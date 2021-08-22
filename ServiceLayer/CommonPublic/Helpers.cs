using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace ServiceLayer
{
    public class Helpers
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW( [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
        
        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);



        /// <summary>
        /// Credits: https://stackoverflow.com/a/49214724
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string[] ParseCommandLineArgs(string input)
        {
            var ptrToSplitArgs = CommandLineToArgvW(input, out var numberOfArgs);
          
            // CommandLineToArgvW returns NULL upon failure.
            if (ptrToSplitArgs == IntPtr.Zero)
                throw new ArgumentException("Unable to split argument.", new Win32Exception());
          

            // Make sure the memory ptrToSplitArgs to is freed, even upon failure.
            try
            {
                var splitArgs = new string[numberOfArgs];

                // ptrToSplitArgs is an array of pointers to null terminated Unicode strings.
                // Copy each of these strings into our split argument array.
                for (var i = 0; i < numberOfArgs; i++)
                {
                    splitArgs[i] = Marshal.PtrToStringUni(
                        Marshal.ReadIntPtr(ptrToSplitArgs, i * IntPtr.Size));
                }
                    
                return splitArgs;
            }
            finally
            {
                // Free memory obtained by CommandLineToArgW.
                LocalFree(ptrToSplitArgs);
            }
        }
        

        public static string[] ToArray(object obj)
        {
            return ((IEnumerable) obj)?.Cast<object>()
                .Select(x => x.ToString())
                .ToArray();
        }

        public static DateTime? ParseAdDateTime(object obj)
        {
            if (obj == null)
                return null;

            long.TryParse(obj.ToString(), out var lastLogonStr);
            return DateTime.FromFileTime(lastLogonStr);
        }

    }
}