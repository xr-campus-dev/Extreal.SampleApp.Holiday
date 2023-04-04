﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Extreal.Core.Common.System;
using Extreal.Core.Logging;
using Extreal.Integration.AssetWorkflow.Addressables;
using Extreal.Integration.Multiplay.NGO;
using Extreal.SampleApp.Holiday.App.AssetWorkflow;
using Extreal.SampleApp.Holiday.App.Avatars;
using Extreal.SampleApp.Holiday.Common.Multiplay;
using UniRx;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extreal.SampleApp.Holiday.Controls.MultiplayControl
{
    public class MultiplayRoom : DisposableBase
    {
        public IObservable<GameObject> IsPlayerSpawned => isPlayerSpawned;
        [SuppressMessage("Usage", "CC0033")]
        private readonly ReactiveProperty<GameObject> isPlayerSpawned = new ReactiveProperty<GameObject>();

        private readonly NgoClient ngoClient;
        private readonly NgoConfig ngoConfig;
        private readonly AssetHelper assetHelper;

        [SuppressMessage("Usage", "CC0033")]
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        [SuppressMessage("Usage", "CC0033")]
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly Dictionary<string, AssetDisposable<GameObject>> loadedAvatars
            = new Dictionary<string, AssetDisposable<GameObject>>();

        private static Dictionary<ulong, NetworkObject> SpawnedObjects
            => NetworkManager.Singleton.SpawnManager.SpawnedObjects;

        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(MultiplayRoom));

        public MultiplayRoom(NgoClient ngoClient, NgoConfig ngoConfig, AssetHelper assetHelper, string avatarAssetName)
        {
            this.ngoClient = ngoClient;
            this.ngoConfig = ngoConfig;
            this.assetHelper = assetHelper;

            this.ngoClient.OnConnected
                .Subscribe(_ =>
                {
                    ngoClient.RegisterMessageHandler(MessageName.PlayerSpawned.ToString(), PlayerSpawnedMessageHandler);
                    SendPlayerSpawn(avatarAssetName);
                })
                .AddTo(disposables);

            this.ngoClient.OnDisconnecting
                .Subscribe(_ =>
                {
                    isPlayerSpawned.Value = null;
                    ngoClient.UnregisterMessageHandler(MessageName.PlayerSpawned.ToString());
                })
                .AddTo(disposables);

            this.ngoClient.OnUnexpectedDisconnected
                .Subscribe(_ => isPlayerSpawned.Value = null)
                .AddTo(disposables);
        }

        protected override void ReleaseManagedResources()
        {
            cts.Cancel();
            cts.Dispose();
            isPlayerSpawned.Dispose();
            disposables.Dispose();
        }

        public async UniTaskVoid JoinAsync() => await ngoClient.ConnectAsync(ngoConfig, cts.Token);

        public async UniTaskVoid LeaveAsync() => await ngoClient.DisconnectAsync();

        private void SendPlayerSpawn(string avatarAssetName)
        {
            if (Logger.IsDebug())
            {
                Logger.LogDebug($"spawn: avatarAssetName: {avatarAssetName}");
            }

            var messageStream = new FastBufferWriter(FixedString64Bytes.UTF8MaxLengthInBytes, Allocator.Temp);
            messageStream.WriteValueSafe(avatarAssetName);
            ngoClient.SendMessage(MessageName.PlayerSpawn.ToString(), messageStream);
        }

        private void PlayerSpawnedMessageHandler(ulong senderClientId, FastBufferReader messagePayload)
        {
            messagePayload.ReadValueSafe(out SpawnedMessage spawnedMessage);
            var spawnedObject = SpawnedObjects[spawnedMessage.NetworkObjectId];
            if (spawnedObject.IsOwner)
            {
                HandleOwnerAsync(spawnedMessage, spawnedObject).Forget();
            }
            else
            {
                SetAvatarAsync(spawnedObject, spawnedMessage.AvatarAssetName).Forget();
            }
        }

        private async UniTaskVoid HandleOwnerAsync(SpawnedMessage spawnedMessage, NetworkObject spawnedObject)
        {
            Controller(spawnedObject).AvatarAssetName.Value = spawnedMessage.AvatarAssetName;
            SetAvatarForExistingSpawnedObjects(ownerId: spawnedMessage.NetworkObjectId);
            isPlayerSpawned.Value = await SetAvatarAsync(spawnedObject, spawnedMessage.AvatarAssetName);
        }

        private static NetworkThirdPersonController Controller(NetworkObject networkObject)
            => networkObject.GetComponent<NetworkThirdPersonController>();

        private void SetAvatarForExistingSpawnedObjects(ulong ownerId)
        {
            foreach (var existingObject in SpawnedObjects.Values.ToArray())
            {
                if (ownerId != existingObject.NetworkObjectId)
                {
                    string avatarName = Controller(existingObject).AvatarAssetName.Value;
                    SetAvatarAsync(existingObject, avatarName, restore: true).Forget();
                }
            }
        }

        private async UniTask<GameObject> SetAvatarAsync(NetworkObject networkObject, string avatarAssetName, bool restore = false)
        {
            var assetDisposable = await LoadAvatarAsync(avatarAssetName);
            var avatarObject = Object.Instantiate(assetDisposable.Result, networkObject.transform);
            Controller(networkObject).SetAvatar(avatarObject.GetComponent<AvatarProvider>().Avatar, restore);
            return avatarObject;
        }

        public async UniTask<AssetDisposable<GameObject>> LoadAvatarAsync(string avatarAssetName)
        {
            if (!loadedAvatars.TryGetValue(avatarAssetName, out var assetDisposable))
            {
                assetDisposable = await assetHelper.LoadAssetAsync<GameObject>(avatarAssetName);
                if (loadedAvatars.TryAdd(avatarAssetName, assetDisposable))
                {
                    disposables.Add(assetDisposable);
                }
                else
                {
                    assetDisposable.Dispose();
                    assetDisposable = loadedAvatars[avatarAssetName];
                }
            }
            return assetDisposable;
        }
    }
}
