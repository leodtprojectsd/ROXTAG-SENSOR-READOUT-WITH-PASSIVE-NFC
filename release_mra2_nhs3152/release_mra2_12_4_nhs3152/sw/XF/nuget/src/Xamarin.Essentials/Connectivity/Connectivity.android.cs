using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.OS;
using Debug = System.Diagnostics.Debug;

namespace Xamarin.Essentials
{
    public partial class Connectivity
    {
        static ConnectivityBroadcastReceiver conectivityReceiver;

        static void StartListeners()
        {
            Permissions.EnsureDeclared(PermissionType.NetworkState);

            conectivityReceiver = new ConnectivityBroadcastReceiver(OnConnectivityChanged);

#pragma warning disable CS0618 // Type or member is obsolete
            Platform.AppContext.RegisterReceiver(conectivityReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        static void StopListeners()
        {
            if (conectivityReceiver == null)
                return;
            try
            {
                Platform.AppContext.UnregisterReceiver(conectivityReceiver);
            }
            catch (Java.Lang.IllegalArgumentException)
            {
                Debug.WriteLine("Connectivity receiver already unregistered. Disposing of it.");
            }
            conectivityReceiver.Dispose();
            conectivityReceiver = null;
        }

        static NetworkAccess IsBetterAccess(NetworkAccess currentAccess, NetworkAccess newAccess) =>
            newAccess > currentAccess ? newAccess : currentAccess;

        static NetworkAccess PlatformNetworkAccess
        {
            get
            {
                Permissions.EnsureDeclared(PermissionType.NetworkState);

                try
                {
                    var currentAccess = NetworkAccess.None;
                    var manager = Platform.ConnectivityManager;

                    if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                    {
                        foreach (var network in manager.GetAllNetworks())
                        {
                            try
                            {
                                var capabilities = manager.GetNetworkCapabilities(network);

                                if (capabilities == null)
                                    continue;

                                var info = manager.GetNetworkInfo(network);

#pragma warning disable CS0618 // Type or member is obsolete
                                if (info == null || !info.IsAvailable)
#pragma warning restore CS0618 // Type or member is obsolete
                                    continue;

                                // Check to see if it has the internet capability
                                if (!capabilities.HasCapability(NetCapability.Internet))
                                {
                                    // Doesn't have internet, but local is possible
                                    currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Local);
                                    continue;
                                }

                                ProcessNetworkInfo(info);
                            }
                            catch
                            {
                                // there is a possibility, but don't worry
                            }
                        }
                    }
                    else
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        foreach (var info in manager.GetAllNetworkInfo())
#pragma warning restore CS0618 // Type or member is obsolete
                        {
                            ProcessNetworkInfo(info);
                        }
                    }

                    void ProcessNetworkInfo(NetworkInfo info)
                    {
#pragma warning disable CS0618 // Type or member is obsolete
                        if (info == null || !info.IsAvailable)
#pragma warning restore CS0618 // Type or member is obsolete
                            return;

                        if (info.IsConnected)
                            currentAccess = IsBetterAccess(currentAccess, NetworkAccess.Internet);
#pragma warning disable CS0618 // Type or member is obsolete
                        else if (info.IsConnectedOrConnecting)
#pragma warning restore CS0618 // Type or member is obsolete
                            currentAccess = IsBetterAccess(currentAccess, NetworkAccess.ConstrainedInternet);
                    }

                    return currentAccess;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unable to get connected state - do you have ACCESS_NETWORK_STATE permission? - error: {0}", e);
                    return NetworkAccess.Unknown;
                }
            }
        }

        static IEnumerable<ConnectionProfile> PlatformConnectionProfiles
        {
            get
            {
                Permissions.EnsureDeclared(PermissionType.NetworkState);

                var manager = Platform.ConnectivityManager;
                if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                {
                    foreach (var network in manager.GetAllNetworks())
                    {
                        NetworkInfo info = null;
                        try
                        {
                            info = manager.GetNetworkInfo(network);
                        }
                        catch
                        {
                            // there is a possibility, but don't worry about it
                        }

                        var p = ProcessNetworkInfo(info);
                        if (p.HasValue)
                            yield return p.Value;
                    }
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    foreach (var info in manager.GetAllNetworkInfo())
#pragma warning restore CS0618 // Type or member is obsolete
                    {
                        var p = ProcessNetworkInfo(info);
                        if (p.HasValue)
                            yield return p.Value;
                    }
                }

                ConnectionProfile? ProcessNetworkInfo(NetworkInfo info)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if (info == null || !info.IsAvailable || !info.IsConnectedOrConnecting)
                        return null;

                    return GetConnectionType(info.Type, info.TypeName);
#pragma warning restore CS0618 // Type or member is obsolete
                }
            }
        }

        internal static ConnectionProfile GetConnectionType(ConnectivityType connectivityType, string typeName)
        {
            switch (connectivityType)
            {
                case ConnectivityType.Ethernet:
                    return ConnectionProfile.Ethernet;
                case ConnectivityType.Wifi:
                    return ConnectionProfile.WiFi;
                case ConnectivityType.Bluetooth:
                    return ConnectionProfile.Bluetooth;
                case ConnectivityType.Wimax:
                case ConnectivityType.Mobile:
                case ConnectivityType.MobileDun:
                case ConnectivityType.MobileHipri:
                case ConnectivityType.MobileMms:
                    return ConnectionProfile.Cellular;
                case ConnectivityType.Dummy:
                    return ConnectionProfile.Unknown;
                default:
                    if (string.IsNullOrWhiteSpace(typeName))
                        return ConnectionProfile.Unknown;

                    var typeNameLower = typeName.ToLowerInvariant();
                    if (typeNameLower.Contains("mobile"))
                        return ConnectionProfile.Cellular;

                    if (typeNameLower.Contains("wimax"))
                        return ConnectionProfile.Cellular;

                    if (typeNameLower.Contains("wifi"))
                        return ConnectionProfile.WiFi;

                    if (typeNameLower.Contains("ethernet"))
                        return ConnectionProfile.Ethernet;

                    if (typeNameLower.Contains("bluetooth"))
                        return ConnectionProfile.Bluetooth;

                    return ConnectionProfile.Unknown;
            }
        }
    }

    [BroadcastReceiver(Enabled = true, Exported = false, Label = "Essentials Connectivity Broadcast Receiver")]
    class ConnectivityBroadcastReceiver : BroadcastReceiver
    {
        Action onChanged;

        public ConnectivityBroadcastReceiver()
        {
        }

        public ConnectivityBroadcastReceiver(Action onChanged) =>
            this.onChanged = onChanged;

        public override async void OnReceive(Context context, Intent intent)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if (intent.Action != ConnectivityManager.ConnectivityAction)
#pragma warning restore CS0618 // Type or member is obsolete
                return;

            // await 1500ms to ensure that the the connection manager updates
            await Task.Delay(1500);
            onChanged?.Invoke();
        }
    }
}
