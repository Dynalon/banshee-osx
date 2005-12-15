/* -*- Mode: csharp; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: t -*- */
/***************************************************************************
 *  StockIcons.cs
 *
 *  Copyright (C) 2005 Novell
 *  Written by Aaron Bockover (aaron@aaronbock.net)
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using Gtk;
using Gdk;
using Mono.Unix;
using System.Collections;

namespace Banshee
{
    public class StockIcons 
    {
        private static string [] stock_icon_names = {
            /* Playback Control Icons */
            "media-skip-forward",
            "media-skip-backward",
            "media-playback-start",
            "media-playback-pause",
            "media-playlist-shuffle",
            "media-playlist-continuous",
            "media-repeat-none",
            "media-repeat-all",
            "media-repeat-single", 
            "media-eject",
            
            /* Volume Button Icons */
            "audio-volume-high", 
            "audio-volume-medium",
            "audio-volume-low", 
            "audio-volume-muted",
            "audio-volume-decrease",
            "audio-volume-increase",
            
            /* Now Playing Images */
            "icon-artist",
            "icon-album",
            "icon-title",
            
            /* Other */
            "cd-action-burn",
            "cd-action-rip",
        };    

        private static void AddResourceToIconSet(string stockId, int size, IconSize iconSize, IconSet iconSet)
        {
            try {
                IconSource source = new IconSource();
                source.Pixbuf = Pixbuf.LoadFromResource(stockId + "-" + size.ToString() + ".png");
                source.Size = iconSize;
                iconSet.AddSource(source);
            } catch(Exception) {
            }
        }
        
        private static void AddThemeIconToIconSet(string stockId, IconSize iconSize, IconSet iconSet)
        {
            try {
                IconSource source = new IconSource();
                source.IconName = stockId;
                source.Size = iconSize;
                iconSet.AddSource(source);
            } catch(Exception) {
            }
        }

        public static void Initialize()
        {
            IconFactory icon_factory = new IconFactory();
            icon_factory.AddDefault();

            foreach(string item_id in stock_icon_names) {
                StockItem item = new StockItem(item_id, null, 0, Gdk.ModifierType.ShiftMask, null);
                
                IconSet icon_set = null; 
                
                if(Banshee.Base.IconThemeUtils.HasIcon(item.StockId)) {
                    // map available icons from the icon theme to stock 
                    icon_set = new IconSet();
                    AddThemeIconToIconSet(item.StockId, IconSize.Menu, icon_set);
                    AddThemeIconToIconSet(item.StockId, IconSize.SmallToolbar, icon_set);
                    AddThemeIconToIconSet(item.StockId, IconSize.Dialog, icon_set);
                } else {
                    // icon wasn't available in the theme, try to load it as stock from a resource file
                    Pixbuf default_pixbuf = null;
                
                    foreach(string postfix in new string [] { "", "-16", "-24", "-48" }) {
                        try {
                            default_pixbuf = Pixbuf.LoadFromResource(item.StockId + postfix + ".png");
                            break;
                        } catch(Exception) {
                            continue;
                        }
                    }
                    
                    icon_set = new IconSet(default_pixbuf);
                    AddResourceToIconSet(item.StockId, 16, IconSize.Menu, icon_set);
                    AddResourceToIconSet(item.StockId, 24, IconSize.SmallToolbar, icon_set);
                    AddResourceToIconSet(item.StockId, 48, IconSize.Dialog, icon_set);
                }
                
                icon_factory.Add(item.StockId, icon_set);
                StockManager.Add(item);
            }
        }
    }
}
