using System;
using System.Collections;
using System.Collections.Generic;

using Banshee.Base;
using Banshee.Cdrom;
using Banshee.Cdrom.Windows.Interop;

namespace Banshee.Cdrom.Windows
{
	class WindowsDriveFactory : DriveFactory
	{
        public WindowsDriveFactory()
        {
            char [] drive_letters = CDDrive.GetCDDriveLetters();
            drives = new Dictionary<string, IDrive>(drive_letters.Length);
            foreach (char c in drive_letters) {
                WindowsDrive drive = new WindowsDrive(c);
                drive.MediaAdded += new MediaHandler(drive_MediaAdded);
                drive.MediaRemoved += new MediaHandler(drive_MediaRemoved);
                drives.Add(drive.Device, drive);

                AudioCdDisk disk = CheckForAudioCdDisk(drive);
                if(disk != null) {
                    OnAudioCdDiskAdded(this, disk);
                }

                HandleUpdated();
            }
        }

        private AudioCdDisk CheckForAudioCdDisk(WindowsDrive drive)
        {
            if(drive.CheckForAudioCdDisk()) {
                AudioCdDisk disk = new WindowsAudioCdDisk(drive);
                disk.Updated += OnAudioCdDiskUpdated;
                if(disk.Valid && !disks.ContainsKey(disk.Udi)) {
                    disks.Add(drive.Device, disk);
                }
                return disk;
            }
            return null;
        }

        private void drive_MediaAdded(object o, MediaArgs args)
        {
            OnMediaAdded(o, args);

            AudioCdDisk disk = CheckForAudioCdDisk(o as WindowsDrive);
            if(disk != null) {
                OnAudioCdDiskAdded(this, disk);
            }

            HandleUpdated();
        }
        
        private void drive_MediaRemoved(object o, MediaArgs args)
        {
            OnMediaRemoved(o, args);
            WindowsDrive drive = o as WindowsDrive;
            if(disks.ContainsKey(drive.Device)) {
                OnAudioCdDiskRemoved(this, disks[drive.Device].Udi);
                disks.Remove(drive.Device);
            }

            HandleUpdated();
        }

        public override int RecorderCount
        {
            get { return 0; } //FIXME wire this up
        }
    }
}
