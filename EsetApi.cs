using System;
using System.Runtime.InteropServices;

namespace EsetServerApiWrapper
{
    public class EsetApi : IDisposable
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "LoadLibrary", SetLastError = true)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int InitLib();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int DeInitLib();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SendReq(string req, out IntPtr resp);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int FreeResponse(IntPtr resp);

        private readonly IntPtr _hMod;
        private readonly IntPtr _initLibPtr;
        private readonly IntPtr _deinitLibPtr;
        private readonly IntPtr _sendReqPtr;
        private readonly IntPtr _freeResponsePtr;
        private readonly InitLib _initLib;
        private readonly DeInitLib _deinitLib;
        private readonly SendReq _sendRequest;
        private readonly FreeResponse _freeResponse;

        private bool _disposed = false;

        public EsetApi()
        {
            // Load Library and Get Pointers for Functions
            _hMod = LoadLibrary("ServerApi.dll");
            _initLibPtr = GetProcAddress(_hMod, "era_init_lib");
            _deinitLibPtr = GetProcAddress(_hMod, "era_deinit_lib");
            _sendReqPtr = GetProcAddress(_hMod, "era_process_request");
            _freeResponsePtr = GetProcAddress(_hMod, "era_free");

            // Establish the Delegates for Function Pointers
            _initLib = Marshal.GetDelegateForFunctionPointer<InitLib>(_initLibPtr);
            _deinitLib = Marshal.GetDelegateForFunctionPointer<DeInitLib>(_deinitLibPtr);
            _sendRequest = Marshal.GetDelegateForFunctionPointer<SendReq>(_sendReqPtr);
            _freeResponse = Marshal.GetDelegateForFunctionPointer<FreeResponse>(_freeResponsePtr);

            // Init the Library
            _initLib();
        }

        public EsetApi(string dllDirectory)
        {
            // Set the Location to Load Unmanaged Libraries
            SetDllDirectory(dllDirectory);

            // Load Library and Get Pointers for Functions
            _hMod = LoadLibrary("ServerApi.dll");
            _initLibPtr = GetProcAddress(_hMod, "era_init_lib");
            _deinitLibPtr = GetProcAddress(_hMod, "era_deinit_lib");
            _sendReqPtr = GetProcAddress(_hMod, "era_process_request");
            _freeResponsePtr = GetProcAddress(_hMod, "era_free");

            // Establish the Delegates for Function Pointers
            _initLib = Marshal.GetDelegateForFunctionPointer<InitLib>(_initLibPtr);
            _deinitLib = Marshal.GetDelegateForFunctionPointer<DeInitLib>(_deinitLibPtr);
            _sendRequest = Marshal.GetDelegateForFunctionPointer<SendReq>(_sendReqPtr);
            _freeResponse = Marshal.GetDelegateForFunctionPointer<FreeResponse>(_freeResponsePtr);

            // Init the Library
            _initLib();
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new TypeUnloadedException("Class Disposed");
        }

        public string SendRequest(string request)
        {
            CheckDisposed();

            _sendRequest(request, out IntPtr resp);

            string response = Marshal.PtrToStringAnsi(resp);

            _freeResponse(resp);

            return response;
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _deinitLib();
            }

            bool freeLib = FreeLibrary(_hMod);

            if (freeLib)
                _disposed = true;
        }
    }
}