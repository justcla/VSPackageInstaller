using Microsoft.VisualStudio.Shell;
using System;
using VSPackageInstaller.Cache;
using System.Diagnostics;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.ExtensionManager;
using System.Linq;
using System.Windows.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace VSPackageInstaller.PackageInstaller
{
    public sealed class PackageInstaller
    {
        private const string Caption = "VS Package Installer";

        public void InstallPackage(IExtensionDataItemView extension)
        {
            //System.Threading.Tasks.Task.Run(ManualInstallExtensionAsync);
            //System.Threading.Tasks.Task.Run(ExtMgrInstallExtensionAsync);

            // Confirm Installation operation
            string text = $"Install extension: {extension.Title}";
            var answer = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, text, Caption, OLEMSGICON.OLEMSGICON_QUERY, OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            if (answer != (int)System.Windows.MessageBoxResult.OK)
            {
                return;
            }

            ThreadHelper.Generic.BeginInvoke(DispatcherPriority.SystemIdle, async () =>
            {
                try
                {
                    await InstallAsync(extension);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }
            });
        }

        private async System.Threading.Tasks.Task ManualInstallExtensionAsync(IExtensionDataItemView extension)
        {
            try
            {
                var fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.vsix");
                if (extension.VsixId == null)
                {   // this is not a VsiX extension it is most probably an MSI

                    fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.msi");
                }
                else
                {   // this is a VsiX extension

                    fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.VsiX");
                }

                using (var webClient = new System.Net.WebClient())
                {
                    // TODO: a good citizen would keep a list of known temp artifacts (like this one) and delete it on next start up.

                    Logger.Log("Marketplace OK"); // Marketplace ok

                    if (extension.Installer != null)
                    {
                        Logger.Log("  " + "Downloading");
                        await webClient.DownloadFileTaskAsync(extension.Installer, fileName);
                        Logger.Log("Downloading OK"); // Download ok

                        // Use the default windows file associations to invoke VSIXinstaller.exe or msi installer, since we don't know the path.
                        Logger.Log("  " + "Installing");

                        Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });

                        Logger.Log("Install OK"); // Install ok
                    }
                    else
                    {
                        Logger.Log("Opening download page for the user to manually download and install"); // Download ok
                                                                                                           // We cannot install this extension directly. Take the user to the download page.
                        Process.Start(new ProcessStartInfo(extension.Link) { UseShellExecute = true });
                    }
                }
            }

            catch (Exception ex)
            {

                // TODO: perhaps we should handle specific exceptions and give custom error messages.

                Logger.Log("Install failed exception:" + ex.ToString());
            }
        }

        private async System.Threading.Tasks.Task ExtMgrInstallExtensionAsync(IExtensionDataItemView extension)
        {
            GalleryEntry entry = null;
            try
            {
                // Waits for MEF to initialize before the extension manager is ready to use
                var scComponentModel = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SComponentModel));

                IVsExtensionRepository repository = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionRepository)) as IVsExtensionRepository;
                IVsExtensionManager manager = VSPackageInstaller.VSPackage.GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager;

                Logger.Log($"{Environment.NewLine}{extension.Title}");
                Logger.Log("  " + "Verifying ", false);


                entry = repository.GetVSGalleryExtensions<GalleryEntry>(new System.Collections.Generic.List<string> { extension.VsixId.ToString() }, 1033, false)?.FirstOrDefault();

                if (entry != null)
                {
                    // ensure that we update the URL if it is empty
                    if (entry.DownloadUrl == null)
                    {
                        entry.DownloadUrl = extension.Installer;
                        throw new Exception("This is not a VsiX");
                    }

                    Logger.Log("Marketplace OK"); // Marketplace ok
                    Logger.Log("  " + "Downloading");

                    IInstallableExtension installable = repository.Download(entry);
                    if (installable == null)
                        throw new Exception("This is not a VsiX");

                    Logger.Log("Downloading OK"); // Download ok
                    Logger.Log("  " + "Installing");
                    manager.Install(installable, false);
                    Logger.Log("Installation queued OK"); // Install ok
                    Logger.Log("This extension package will now be automatically installed when you exist all instances of Visual Studio");
                }
                else
                {
                    Logger.Log("Marketplace failed"); // Markedplace failed
                    // This is not a VsiX
                    throw new Exception("This is not a VsiX");
                    //Logger.Log("Done");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Extension Manager installtion failed exception: " + ex);
                Logger.Log("Trying manual downloand and install");
                //await this.ManualInstallExtensionAsync(extension);
            }
            finally
            {
                await System.Threading.Tasks.Task.Yield();
            }
        }

        private async System.Threading.Tasks.Task InstallAsync(IExtensionDataItemView extension)
        {
            var repository = (IVsExtensionRepository)Package.GetGlobalService(typeof(SVsExtensionRepository));
            var manager = (IVsExtensionManager)Package.GetGlobalService(typeof(SVsExtensionManager));

            await System.Threading.Tasks.Task.Run(() =>
            {
                InstallExtension(repository, manager, extension);
            });

        }

        private void InstallExtension(IVsExtensionRepository repository, IVsExtensionManager manager, IExtensionDataItemView extension)
        {
            GalleryEntry entry = null;
            try
            {
                entry = repository.CreateQuery<GalleryEntry>(includeTypeInQuery: false, includeSkuInQuery: true, searchSource: "VSPackageInstaller-Install")
                                                                                 .Where(e => e.VsixID == extension.VsixId)
                                                                                 .AsEnumerable().FirstOrDefault();
                if (entry != null)
                {
                    var installable = repository.Download(entry);
                    manager.Install(installable, false);
                    // Success! Offer restart operation
                    PromptForRestart(extension.Title);
                }
                else
                {
                    string text = $"Unable to fetch extension from Marketplace: {extension.Title}";
                    VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, text, Caption, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                string text = $"Failed to install extension: {extension.Title}\n\nError: {ex.Message}";
                VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, text, Caption, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private static void PromptForRestart(string extensionTitle)
        {
            string prompt = $"Successfully installed extension: {extensionTitle}\r\rChanges will take effect next time Visual Studio is started.\rDo you want to restart Visual Studio now?";
            int answer = VsShellUtilities.ShowMessageBox(ServiceProvider.GlobalProvider, prompt, Caption, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND);
            if (answer == (int)System.Windows.MessageBoxResult.OK)
            {
                IVsShell4 shell = (IVsShell4)Package.GetGlobalService(typeof(SVsShell));
                shell.Restart((uint)__VSRESTARTTYPE.RESTART_Normal);
            }
        }

    }
}
