using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Share
    {
        static Task PlatformRequestAsync(ShareTextRequest request) =>
            throw new System.PlatformNotSupportedException();

        static Task PlatformRequestAsync(ShareFileRequest request) =>
            throw new System.PlatformNotSupportedException();
    }
}
