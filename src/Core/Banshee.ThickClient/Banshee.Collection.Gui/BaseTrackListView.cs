//
// BaseTrackListView.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007-2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Mono.Unix;
using Gtk;

using Hyena.Data;
using Hyena.Data.Gui;
using Hyena.Gui;

using Banshee.Sources;
using Banshee.ServiceStack;
using Banshee.MediaEngine;
using Banshee.Playlist;

using Banshee.Gui;

namespace Banshee.Collection.Gui
{
    public class BaseTrackListView : ListView<TrackInfo>
    {
        public BaseTrackListView () : base ()
        {
            RulesHint = true;
            RowSensitivePropertyName = "CanPlay";
            RowBoldPropertyName = "IsPlaying";
            
            ServiceManager.PlayerEngine.ConnectEvent (OnPlayerEvent, PlayerEvent.StateChange);
            
            ForceDragSourceSet = true;
            Reorderable = true;
            
            RowActivated += delegate (object o, RowActivatedArgs<TrackInfo> args) {
                ITrackModelSource source = ServiceManager.SourceManager.ActiveSource as ITrackModelSource;
                if (source != null && source.TrackModel == Model) {
                    ServiceManager.PlaybackController.Source = source;
                    ServiceManager.PlayerEngine.OpenPlay (args.RowValue);
                }
            };
        }

        private static TargetEntry [] source_targets = new TargetEntry [] {
            ListViewDragDropTarget.ModelSelection,
            Banshee.Gui.DragDrop.DragDropTarget.UriList
        };

        protected override TargetEntry [] DragDropSourceEntries {
            get { return source_targets; }
        }
        
        protected override bool OnKeyPressEvent (Gdk.EventKey press)
        {
            // Have o act the same as enter - activate the selection
            if (GtkUtilities.NoImportantModifiersAreSet () && press.Key == Gdk.Key.o && ActivateSelection ()) {
                return true;
            }
            return base.OnKeyPressEvent (press);
        }

        protected override bool OnPopupMenu ()
        {
            ServiceManager.Get<InterfaceActionService> ().TrackActions["TrackContextMenuAction"].Activate ();
            return true;
        }
        
        private void OnPlayerEvent (PlayerEventArgs args)
        {
            QueueDraw ();
        }

#region Drag and Drop

        protected override void OnDragSourceSet ()
        {
            base.OnDragSourceSet ();
            Drag.SourceSetIconName (this, "audio-x-generic");
        }
        
        protected override bool OnDragDrop (Gdk.DragContext context, int x, int y, uint time_)
        {
            y = TranslateToListY (y);
            if (Gtk.Drag.GetSourceWidget (context) == this) {
                PlaylistSource playlist = ServiceManager.SourceManager.ActiveSource as PlaylistSource;
                if (playlist != null) {
                    //Gtk.Drag.
                    int row = GetRowAtY (y);
                    if (row != GetRowAtY (y + RowHeight / 2)) {
                        row += 1;
                    }
                    
                    if (playlist.TrackModel.Selection.Contains (row)) {
                        // can't drop within the selection
                        return false;
                    }
                    
                    playlist.ReorderSelectedTracks (row);
                    return true;
                }
            }
            
            return false;
        }

        protected override void OnDragDataGet (Gdk.DragContext context, SelectionData selection_data, uint info, uint time)
        {
            if (info == Banshee.Gui.DragDrop.DragDropTarget.UriList.Info) {
                ITrackModelSource track_source = ServiceManager.SourceManager.ActiveSource as ITrackModelSource;
                if (track_source != null) {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder ();
                    foreach (TrackInfo track in track_source.TrackModel.SelectedItems) {
                        sb.Append (track.Uri);
                        sb.Append ("\r\n");
                    }
                    byte [] data = System.Text.Encoding.UTF8.GetBytes (sb.ToString ());
                    selection_data.Set (context.Targets[0], 8, data, data.Length);
                }
            }
        }

#endregion
    }
}
