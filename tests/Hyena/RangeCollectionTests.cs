//
// RangeCollectionTests.cs
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
using System.Collections.Generic;
using NUnit.Framework;
using Hyena.Collections;

[TestFixture]
public class RangeCollectionTests
{
    [Test]
    public void TestSingleRanges ()
    {
        _TestRanges (new RangeCollection (), new int [] { 1, 11, 5, 7, 15, 32, 3, 9, 34 });
    }
    
    [Test]
    public void TestMergedRanges ()
    {
        RangeCollection range = new RangeCollection ();
        int [] indexes = new int [] { 0, 7, 5, 9, 1, 6, 8, 2, 10, 12 };
        
        _TestRanges (range, indexes);
        Assert.AreEqual (3, range.RangeCount);
        
        int i= 0;
        foreach (RangeCollection.Range r in range.Ranges) {
            switch (i++) {
                case 0:
                    Assert.AreEqual (0, r.Start);
                    Assert.AreEqual (2, r.End);
                    break;
                case 1:
                    Assert.AreEqual (5, r.Start);
                    Assert.AreEqual (10, r.End);
                    break;
                case 2:
                    Assert.AreEqual (12, r.Start);
                    Assert.AreEqual (12, r.End);
                    break;
                default:
                    Assert.Fail ("This should not be reached!");
                    break;
            }
        }
    }
    
    [Test]
    public void TestLargeSequential ()
    { 
        RangeCollection range = new RangeCollection ();
        int i, n = 1000000;
        
        for (i = 0; i < n; i++) {
            range.Add (i);
            Assert.AreEqual (1, range.RangeCount);
        }
        
        Assert.AreEqual (n, range.Count);
        
        i = 0;
        foreach (int j in range) {
            Assert.AreEqual (i++, j);
        }
        
        Assert.AreEqual (n, i);
    }

    [Test]
    public void TestLargeNonAdjacent ()
    { 
        RangeCollection range = new RangeCollection ();
        int i, n = 1000000;
        
        for (i = 0; i < n; i += 2) {
            range.Add (i);
        }
        
        Assert.AreEqual (n / 2, range.Count);
        
        i = 0;
        foreach (int j in range) {
            Assert.AreEqual (i, j);
            i += 2;
        }
        
        Assert.AreEqual (n, i);
    }

    private static void _TestRanges (RangeCollection range, int [] indexes)
    {
        foreach (int index in indexes) {
            range.Add (index);
        }
        
        Assert.AreEqual (indexes.Length, range.Count);
        
        Array.Sort (indexes);
        
        int i = 0;
        foreach (int index in range) {
            Assert.AreEqual (indexes[i++], index);
        }
        
#pragma warning disable 0618

        i = 0;
        foreach (int index in range.Indexes) {
            Assert.AreEqual (indexes[i++], index);
        }
        
        for (i = 0; i < range.Indexes.Length; i++) {
            Assert.AreEqual (indexes[i], range.Indexes[i]);
        }

#pragma warning restore 0618

    }
    
    [Test]
    public void TestRemoveSingles ()
    {
        RangeCollection range = new RangeCollection ();
        int [] indexes = new int [] { 0, 2, 4, 6, 8, 10, 12, 14 };
        foreach (int index in indexes) {
            range.Add (index);
        }
        
        foreach (int index in indexes) {
            Assert.AreEqual (true, range.Remove (index));
        }
    }
    
    [Test]
    public void TestRemoveStarts ()
    {
        RangeCollection range = _SetupTestRemoveMerges ();
        
        Assert.AreEqual (true, range.Contains (0));
        range.Remove (0);
        Assert.AreEqual (false, range.Contains (0));
        Assert.AreEqual (4, range.RangeCount);
        
        Assert.AreEqual (true, range.Contains (2));
        range.Remove (2);
        Assert.AreEqual (false, range.Contains (2));
        Assert.AreEqual (4, range.RangeCount);
        Assert.AreEqual (3, range.Ranges[0].Start);
        Assert.AreEqual (5, range.Ranges[0].End);
        
        Assert.AreEqual (true, range.Contains (14));
        range.Remove (14);
        Assert.AreEqual (false, range.Contains (14));
        Assert.AreEqual (4, range.RangeCount);
        Assert.AreEqual (15, range.Ranges[2].Start);
        Assert.AreEqual (15, range.Ranges[2].End);
    }
     
    [Test]
    public void TestRemoveEnds ()
    {
        RangeCollection range = _SetupTestRemoveMerges ();
        
        Assert.AreEqual (true, range.Contains (5));
        range.Remove (5);
        Assert.AreEqual (false, range.Contains (5));
        Assert.AreEqual (5, range.RangeCount);
        Assert.AreEqual (2, range.Ranges[1].Start);
        Assert.AreEqual (4, range.Ranges[1].End);
        
        Assert.AreEqual (true, range.Contains (15));
        range.Remove (15);
        Assert.AreEqual (false, range.Contains (15));
        Assert.AreEqual (5, range.RangeCount);
        Assert.AreEqual (14, range.Ranges[3].Start);
        Assert.AreEqual (14, range.Ranges[3].End);
    }
    
    [Test]
    public void TestRemoveMids ()
    {
        RangeCollection range = _SetupTestRemoveMerges ();
        
        Assert.AreEqual (5, range.RangeCount);
        Assert.AreEqual (14, range.Ranges[3].Start);
        Assert.AreEqual (15, range.Ranges[3].End);
        Assert.AreEqual (true, range.Contains (9));
        range.Remove (9);
        Assert.AreEqual (false, range.Contains (9));
        Assert.AreEqual (6, range.RangeCount);
        Assert.AreEqual (7, range.Ranges[2].Start);
        Assert.AreEqual (8, range.Ranges[2].End);
        Assert.AreEqual (10, range.Ranges[3].Start);
        Assert.AreEqual (11, range.Ranges[3].End);
        Assert.AreEqual (14, range.Ranges[4].Start);
        Assert.AreEqual (15, range.Ranges[4].End);
    }
    
    private static RangeCollection _SetupTestRemoveMerges ()
    {
        RangeCollection range = new RangeCollection ();
        int [] indexes = new int [] { 
            0, 
            2, 3, 4, 5,
            7, 8, 9, 10, 11,
            14, 15,
            17, 18, 19
        };
        
        foreach (int index in indexes) {
            range.Add (index);
        }
        
        int i = 0;
        foreach (RangeCollection.Range r in range.Ranges) {
            switch (i++) {
                case 0:
                    Assert.AreEqual (0, r.Start);
                    Assert.AreEqual (0, r.End);
                    break;
                case 1:
                    Assert.AreEqual (2, r.Start);
                    Assert.AreEqual (5, r.End);
                    break;
                case 2:
                    Assert.AreEqual (7, r.Start);
                    Assert.AreEqual (11, r.End);
                    break;
                case 3:
                    Assert.AreEqual (14, r.Start);
                    Assert.AreEqual (15, r.End);
                    break;
                case 4:
                    Assert.AreEqual (17, r.Start);
                    Assert.AreEqual (19, r.End);
                    break;
                default:
                    Assert.Fail ("Should never reach here");
                    break;
            }
        }
        
        return range;
    }
    
#pragma warning disable 0618
    
    [Test]
    public void TestIndexesCacheGeneration ()
    {
        RangeCollection range = new RangeCollection ();
        int [] index_cache = range.Indexes;
        
        Assert.AreSame (index_cache, range.Indexes);
        
        range.Add (0);
        range.Add (5);
        
        if (index_cache == range.Indexes) {
            Assert.Fail ("Indexes Cache not regenerated after change");
        }
        
        index_cache = range.Indexes;
        range.Remove (0);
        range.Add (3);

        if (index_cache == range.Indexes) {
            Assert.Fail ("Indexes Cache not regenerated after change");
        }
    }
    
#pragma warning restore 0618

    [Test]
    public void TestIndexOf ()
    {
        RangeCollection range = new RangeCollection ();
        
        range.Add (0);
        range.Add (2);
        range.Add (3);
        range.Add (5);
        range.Add (6);
        range.Add (7);
        range.Add (8);
        range.Add (11);
        range.Add (12);
        range.Add (13);
        
        Assert.AreEqual (0, range.IndexOf (0));
        Assert.AreEqual (1, range.IndexOf (2));
        Assert.AreEqual (2, range.IndexOf (3));
        Assert.AreEqual (3, range.IndexOf (5));
        Assert.AreEqual (4, range.IndexOf (6));
        Assert.AreEqual (5, range.IndexOf (7));
        Assert.AreEqual (6, range.IndexOf (8));
        Assert.AreEqual (7, range.IndexOf (11));
        Assert.AreEqual (8, range.IndexOf (12));
        Assert.AreEqual (9, range.IndexOf (13));
        Assert.AreEqual (-1, range.IndexOf (99));
    }
    
    [Test]
    public void TestIndexerForGoodIndexes ()
    {
        RangeCollection range = new RangeCollection ();
        
        /*
        Range  Idx  Value
        0-2    0 -> 0
               1 -> 1
               2 -> 2
        
        7-9    3 -> 7
               4 -> 8
               5 -> 9
        
        11-13  6 -> 11
               7 -> 12
               8 -> 13
        */
        
        range.Add (0);
        range.Add (1);
        range.Add (2);
        range.Add (7);
        range.Add (8);
        range.Add (9);
        range.Add (11);
        range.Add (12);
        range.Add (13);
        
        Assert.AreEqual (0, range[0]);
        Assert.AreEqual (1, range[1]);
        Assert.AreEqual (2, range[2]);
        Assert.AreEqual (7, range[3]);
        Assert.AreEqual (8, range[4]);
        Assert.AreEqual (9, range[5]);
        Assert.AreEqual (11, range[6]);
        Assert.AreEqual (12, range[7]);
        Assert.AreEqual (13, range[8]);
    }
    
    [Test]
    public void TestStressForGoodIndexes ()
    {
        Random random = new Random (0xbeef);
        RangeCollection ranges = new RangeCollection ();
        List<int> indexes = new List<int> ();
        
        for (int i = 0, n = 250000; i < n; i++) {
            int value = random.Next (n);
            if (ranges.Add (value)) {
                indexes.Add (value);
            }
        }
        
        indexes.Sort ();
        
        Assert.AreEqual (indexes.Count, ranges.Count);
        for (int i = 0; i < indexes.Count; i++) {
            Assert.AreEqual (indexes[i], ranges[i]);
        }
    }
    
    [Test]
    [ExpectedException (typeof (IndexOutOfRangeException))]
    public void TestIndexerForNegativeBadIndex ()
    {
        RangeCollection range = new RangeCollection ();
        Assert.AreEqual (0, range[1]);
    }
    
    [Test]
    [ExpectedException (typeof (IndexOutOfRangeException))]
    public void TestIndexerForZeroBadIndex ()
    {
        RangeCollection range = new RangeCollection ();
        Assert.AreEqual (0, range[0]);
    }
    
    [Test]
    [ExpectedException (typeof (IndexOutOfRangeException))]
    public void TestIndexerForPositiveBadIndex ()
    {
        RangeCollection range = new RangeCollection ();
        range.Add (1);
        Assert.AreEqual (0, range[1]);
    }
    
    [Test]
    public void TestExplicitInterface ()
    {
        ICollection<int> range = new RangeCollection ();
        range.Add (1);
        range.Add (2);
        range.Add (5);
        range.Add (6);
        
        Assert.AreEqual (4, range.Count);
    }
}
