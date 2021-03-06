﻿namespace Comet
{
    #region Namespace

    using System;
    using System.Data;
    using System.IO;

    using Comet.Commands;
    using Comet.Managers;
    using Comet.Structure;

    #endregion

    /// <summary>The <see cref="UpdateManager" />.</summary>
    public class UpdateManager
    {
        #region Variables

        private bool _autoUpdate;
        private string _downloadPath;
        private string _executablePath;
        private string _packageDownloadPath;
        private Uri _packagePath;
        private UpdateState _state;
        private bool _working;

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="UpdateManager" /> class.</summary>
        /// <param name="packagePath">The package url.</param>
        /// <param name="downloadPath">The download folder path.</param>
        /// <param name="executablePath">The destination executable Path.</param>
        /// <param name="autoUpdate">Auto update the application.</param>
        public UpdateManager(Uri packagePath, string downloadPath, string executablePath, bool autoUpdate) : this()
        {
            Initialize(packagePath, downloadPath, executablePath, autoUpdate);
        }

        /// <summary>Initializes a new instance of the <see cref="UpdateManager" /> class.</summary>
        /// <param name="packagePath">The package url.</param>
        public UpdateManager(Uri packagePath) : this()
        {
            Initialize(packagePath, FileManager.CreateTempPath("Update"), ApplicationManager.GetMainModuleFileName(), false);
        }

        /// <summary>Initializes a new instance of the <see cref="UpdateManager" /> class.</summary>
        public UpdateManager()
        {
            _packagePath = null;
            _downloadPath = string.Empty;
            _autoUpdate = false;
            _executablePath = string.Empty;
            _working = false;
            _state = UpdateState.NotChecked;
            _packageDownloadPath = string.Empty;
        }

        /// <summary>The update state.</summary>
        [Serializable]
        public enum UpdateState
        {
            /// <summary>The not checked.</summary>
            NotChecked,

            /// <summary>The updated.</summary>
            Updated,

            /// <summary>The outdated.</summary>
            Outdated,

            /// <summary>The downloading.</summary>
            Downloading
        }

        #endregion

        #region Properties

        /// <summary>The AutoUpdate.</summary>
        public bool AutoUpdate
        {
            get
            {
                return _autoUpdate;
            }

            set
            {
                _autoUpdate = value;
            }
        }

        /// <summary>The download path.</summary>
        public string DownloadPath
        {
            get
            {
                return _downloadPath;
            }

            set
            {
                _downloadPath = value;
            }
        }

        /// <summary>The executable path.</summary>
        public string ExecutablePath
        {
            get
            {
                return _executablePath;
            }

            set
            {
                _executablePath = value;
            }
        }

        /// <summary>The Package download path.</summary>
        public string PackageDownloadPath
        {
            get
            {
                return _packageDownloadPath;
            }

            set
            {
                _packageDownloadPath = value;
            }
        }

        /// <summary>The source url.</summary>
        public Uri PackagePath
        {
            get
            {
                return _packagePath;
            }

            set
            {
                _packagePath = value;
            }
        }

        /// <summary>The update state.</summary>
        public UpdateState State
        {
            get
            {
                return _state;
            }
        }

        #endregion

        #region Events

        /// <summary>Checks for updates.</summary>
        public void CheckForUpdate()
        {
            if (_working)
            {
                throw new InvalidOperationException("Another update process is already in progress.");
            }
            else if (_packagePath == null)
            {
                throw new InvalidOperationException("The source must be set before checking for updates.");
            }
            else if (!NetworkManager.IsURLFormatted(_packagePath.ToString()))
            {
                throw new UriFormatException("The source uri is not well formatted.");
            }
            else if (!NetworkManager.SourceExists(_packagePath.ToString()))
            {
                throw new FileNotFoundException("The remote source file is not found.");
            }
            else if (string.IsNullOrWhiteSpace(_downloadPath))
            {
                throw new NoNullAllowedException("The path is null or whitespace.");
            }

            if (_state != UpdateState.NotChecked)
            {
                throw new InvalidOperationException("Already checked for updates.");
            }

            if (UpdateRequired())
            {
                _state = UpdateState.Outdated;
            }
            else
            {
                _state = UpdateState.Updated;
            }

            DefaultCommands.Check(_executablePath, _packagePath.ToString());
        }

        /// <summary>Cleanup temporary files.</summary>
        public void Cleanup()
        {
            FileManager.DeleteDirectory(_downloadPath);
        }

        /// <summary>Download the update package.</summary>
        public void Download()
        {
            Package _package = new Package(_packagePath.ToString());
            _state = UpdateState.Downloading;

            DefaultCommands.Download(_package.Download, _packageDownloadPath);
        }

        /// <summary>Extract the update package.</summary>
        public void Extract()
        {
            DefaultCommands.Extract(_packageDownloadPath, _downloadPath);
        }

        /// <summary>Install the update package.</summary>
        public void Install()
        {
            // create setup script and run it after closing application.

            // setup script should extract all the file/s?
            // Then it should replace them and start the main executable.
        }

        /// <summary>Check if update required.</summary>
        /// <returns><see cref="bool" />.</returns>
        public bool UpdateRequired()
        {
            return ApplicationManager.CheckForUpdate(_executablePath, _packagePath.ToString());
        }

        /// <summary>Initializes the <see cref="UpdateManager" />.</summary>
        /// <param name="source">The source url.</param>
        /// <param name="downloadPath">The folder path.</param>
        /// <param name="installPath">The install Path.</param>
        /// <param name="autoUpdate">The AutoUpdate toggle.</param>
        private void Initialize(Uri source, string downloadPath, string installPath, bool autoUpdate)
        {
            _packagePath = source;
            _downloadPath = downloadPath;
            _executablePath = installPath;
            _autoUpdate = autoUpdate;

            CheckForUpdate();

            if (UpdateRequired())
            {
                if (_autoUpdate)
                {
                    PrepareUpdate();
                    Download();

                    // Extract(); Wait for download to finish.

                    // TODO: Restart
                    // TODO: Success? Cleanup then.
                }
            }
        }

        /// <summary>Prepare the update package.</summary>
        private void PrepareUpdate()
        {
            FileManager.CreateDirectory(_downloadPath);
            ConsoleManager.WriteOutput("Created download directory.");
            ConsoleManager.WriteOutput("Directory: " + _downloadPath);
        }

        #endregion
    }
}