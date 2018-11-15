using System;
using System.Collections.Generic;
using System.IO;

namespace IASoft.SmartCard.Signer.PkcsUtils
{
    public class EacPkcsLibSearcher : IEacPkcsLibSearcher
    {
        private const string EacPkcsProviderName = "EAC MW klient";
        private const string PkcsLibName = "pkcs11_x86.dll";

        public string FindPcksLib()
        {
            var foldersForSearch = GetFoldersForSearch();
            
            foreach (var folder in foldersForSearch)
            {
                var eacDir = Path.Combine(folder, EacPkcsProviderName);
                if (Directory.Exists(eacDir))
                {
                    return Path.Combine(eacDir, PkcsLibName);
                }
            }
            
            throw new InvalidOperationException("EAC MW Pkcs library wasn't found");
        }
        
        private static IEnumerable<string> GetFoldersForSearch()
        {
            var folders = new List<string>();
            
            if(8 == IntPtr.Size || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                folders.Add(Environment.GetEnvironmentVariable("ProgramFiles(x86)"));
            }

            folders.Add(Environment.GetEnvironmentVariable("ProgramFiles"));

            return folders;
        }
    }
}