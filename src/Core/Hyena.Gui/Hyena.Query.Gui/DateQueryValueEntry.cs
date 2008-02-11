//
// DateQueryValueEntry.cs
//
// Authors:
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
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

using Hyena.Query;
using Gtk;

namespace Hyena.Query.Gui
{
    public class DateQueryValueEntry : QueryValueEntry
    {
        protected SpinButton spin_button;
        protected ComboBox combo;
        protected DateQueryValue query_value;

        protected static readonly RelativeDateFactor [] factors = new RelativeDateFactor [] {
            RelativeDateFactor.Second, RelativeDateFactor.Minute, RelativeDateFactor.Hour, RelativeDateFactor.Day,
            RelativeDateFactor.Week, RelativeDateFactor.Month, RelativeDateFactor.Year
        };

        // Relative: [<|>] [num] [minutes|hours] ago
        // TODO: Absolute: [>|>=|=|<|<=] [date/time]
        public DateQueryValueEntry () : base ()
        {
            spin_button = new SpinButton (0.0, 1.0, 1.0);
            spin_button.Digits = 0;
            spin_button.WidthChars = 4;
            spin_button.SetRange (0.0, Double.MaxValue);
            Add (spin_button);

            combo = ComboBox.NewText ();
            combo.AppendText (Catalog.GetString ("seconds"));
            combo.AppendText (Catalog.GetString ("minutes"));
            combo.AppendText (Catalog.GetString ("hours"));
            combo.AppendText (Catalog.GetString ("days"));
            combo.AppendText (Catalog.GetString ("weeks"));
            combo.AppendText (Catalog.GetString ("months"));
            combo.AppendText (Catalog.GetString ("years"));
            combo.Realized += delegate { combo.Active = 1; };
            Add (combo);

            Add (new Label ("ago"));

            spin_button.ValueChanged += HandleValueChanged;
            combo.Changed += HandleValueChanged;
        }

        public override QueryValue QueryValue {
            get { return query_value; }
            set { 
                spin_button.ValueChanged -= HandleValueChanged;
                combo.Changed -= HandleValueChanged;
                query_value = value as DateQueryValue;
                combo.Active = Array.IndexOf (factors, query_value.Factor);
                spin_button.ValueChanged += HandleValueChanged;
                combo.Changed += HandleValueChanged;
            }
        }

        protected void HandleValueChanged (object o, EventArgs args)
        {
            query_value.SetRelativeValue (-spin_button.ValueAsInt, factors [combo.Active]);
        }
    }
}
