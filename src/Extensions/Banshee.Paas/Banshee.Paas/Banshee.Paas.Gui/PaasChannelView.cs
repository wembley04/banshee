// 
// PaasChannelView.cs
//  
// Authors:
//   Mike Urbanski <michael.c.urbanski@gmail.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2009 Michael C. Urbanski
// Copyright (C) 2008 Novell, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Hyena.Data.Gui;

using Banshee.Gui;
using Banshee.ServiceStack;
using Banshee.Collection.Gui;

using Banshee.Paas.Data;

namespace Banshee.Paas.Gui
{
    public class PaasChannelView : TrackFilterListView<PaasChannel>
    {
        // Awful, dirty, filthy hack.
        // I'm having a similar problem, probably just need to tinker with the event flags... Need to move on for now...
        // http://lists.ximian.com/archives/public/gtk-sharp-list/2006-June/007247.html
        public EventHandler<EventArgs> FuckedPopupMenu;

        private ColumnCellChannel renderer;

        public PaasChannelView ()            
        {
            renderer = new ColumnCellChannel ();
            
            column_controller.Add (new Column ("Channels", renderer, 1.0));
            
            ColumnController  = column_controller;
            RowHeightProvider = renderer.ComputeRowHeight;
        }

        public void SetChannelDataHelper (ColumnCellDataHelper dataHelper)
        {
            renderer.DataHelper = dataHelper;
        }

        protected override bool OnPopupMenu ()
        {
            EventHandler<EventArgs> handler = FuckedPopupMenu;

            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        
            ServiceManager.Get<InterfaceActionService> ().FindAction ("Paas.PaasChannelPopupAction").Activate ();
            
            return true;
        }
    }
}
