using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell;

namespace WebEssentials
{
    public class Installer
    {
        private Progress _progress;

        public Installer()
        {
        }


        private async System.Threading.Tasks.Task InstallAsync(IEnumerable<ExtensionEntry> extensions, IVsExtensionRepository repository, IVsExtensionManager manager, CancellationToken token)
        {
            if (!extensions.Any() || token.IsCancellationRequested)
                return;

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    foreach (ExtensionEntry extension in extensions)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        InstallExtension(extension, repository, manager);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex);
                }
                finally
                {
                }
            }).ConfigureAwait(false);
        }

        private async System.Threading.Tasks.Task UninstallAsync(IEnumerable<ExtensionEntry> extensions, IVsExtensionRepository repository, IVsExtensionManager manager, CancellationToken token)
        {
            if (!extensions.Any() || token.IsCancellationRequested)
                return;

            await System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    foreach (ExtensionEntry extension in extensions)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        string msg = string.Format(Resources.Text.UninstallingExtension, extension.Name);

                        OnUpdate(msg);
                        Logger.Log(msg, false);

                        try
                        {
                            if (manager.TryGetInstalledExtension(extension.Id, out IInstalledExtension installedExtension))
                            {
                                manager.Uninstall(installedExtension);
                                Telemetry.Uninstall(extension.Id, true);

                                Logger.Log(Resources.Text.Ok);
                            }
                        }
                        catch (Exception)
                        {
                            Logger.Log("Failed to uninstall");
                            Telemetry.Uninstall(extension.Id, false);
                        }
                    }
                }
                finally
                {
                }
            });
        }

        public void InstallExtension(ExtensionEntry extension, IVsExtensionRepository repository, IVsExtensionManager manager)
        {
            GalleryEntry entry = null;
           OnUpdate(string.Format("Installing Extension: ", extension.Name));

            try
            {
                Logger.Log($"{Environment.NewLine}{extension.Name}");
                Logger.Log("  " + "Verifying ", false);

                entry = repository.GetVSGalleryExtensions<GalleryEntry>(new List<string> { extension.Id }, 1033, false)?.FirstOrDefault();

                if (entry != null)
                {
                    Logger.Log("Marketplace OK"); // Marketplace ok
                    Logger.Log("  " + "Downloading", false);

                    IInstallableExtension installable = repository.Download(entry);
                    Logger.Log("Downloading OK"); // Download ok
                    Logger.Log("  " + "Installing", false);
                    manager.Install(installable, false);
                    Logger.Log("Install OK"); // Install ok
                    Telemetry.Install(extension.Id, true);
                }
                else
                {
                    Logger.Log("Marketplace failed"); // Markedplace failed
                }
            }
            catch (Exception)
            {
                Logger.Log("Install failed exception");
                Telemetry.Install(extension.Id, false);
            }
            finally
            {
            }
        }

        private void OnUpdate(string text)
        {
            _progress.Current += 1;
            _progress.Text = text;

            Update?.Invoke(this, _progress);
        }

        public event EventHandler<Progress> Update;
        public event EventHandler<int> Done;
    }
}
