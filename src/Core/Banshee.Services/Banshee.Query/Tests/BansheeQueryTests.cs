// 
// BansheeQueryTests.cs
// 
// Author:
//   Andrés G. Aragoneses <knocte@gmail.com>
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

#if ENABLE_TESTS

using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Banshee.Query.Tests
{
    [TestFixture]
    public class GetSortTestsWithAlbumYearOff
    {

        bool original_sort_album_by_year;

        [TestFixtureSetUp]
        public void SetSortAlbumByYearOff ()
        {
            original_sort_album_by_year = Banshee.Configuration.Schema.LibrarySchema.SortByAlbumYear.Get ();
            if (original_sort_album_by_year) {
                Banshee.Configuration.Schema.LibrarySchema.SortByAlbumYear.Set (false);
            }
        }

        [TestFixtureTearDown]
        public void RecoverSortAlbumByYearSetting ()
        {
            Banshee.Configuration.Schema.LibrarySchema.SortByAlbumYear.Set (original_sort_album_by_year);
        }

        private static void AssertAreEquivalent (string expected, string actual)
        {
            Assert.AreEqual (FullTrim (expected), FullTrim (actual));
        }

        private static string FullTrim (string str)
        {
            var r = new Regex (@"\s+");
            return r.Replace (str, " ").Trim ();
        }

        [Test]
        public void GetSortForAddedAsc ()
        {
            string sort = BansheeQuery.GetSort ("Added", true);
            AssertAreEquivalent (@"CoreTracks.dateaddedstamp ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForAddedDesc ()
        {
            string sort = BansheeQuery.GetSort ("Added", false);
            AssertAreEquivalent (@"CoreTracks.dateaddedstamp DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForAlbumArtistAsc ()
        {
            string sort = BansheeQuery.GetSort ("AlbumArtist", true);
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForAlbumArtistDesc ()
        {
            string sort = BansheeQuery.GetSort ("AlbumArtist", false);
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey DESC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForAlbumAsc ()
        {
            string sort = BansheeQuery.GetSort ("Album", true);
            AssertAreEquivalent (@"CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForAlbumDesc ()
        {
            string sort = BansheeQuery.GetSort ("Album", false);
            AssertAreEquivalent (@"CoreAlbums.TitleSortKey DESC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }


        [Test]
        public void GetSortForArtistAsc ()
        {
            string sort = BansheeQuery.GetSort ("Artist", true);
            AssertAreEquivalent (@"CoreArtists.NameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForArtistDesc ()
        {
            string sort = BansheeQuery.GetSort ("Artist", false);
            AssertAreEquivalent (@"CoreArtists.NameSortKey DESC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                  sort);
        }

        [Test]
        public void GetSortForBpmAsc ()
        {
            string sort = BansheeQuery.GetSort ("Bpm", true);
            AssertAreEquivalent (@"CoreTracks.Bpm ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                  sort);
        }

        [Test]
        public void GetSortForBpmDesc ()
        {
            string sort = BansheeQuery.GetSort ("Bpm", false);
            AssertAreEquivalent (@"CoreTracks.Bpm DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForBitRateAsc ()
        {
            string sort = BansheeQuery.GetSort ("BitRate", true);
            AssertAreEquivalent (@"CoreTracks.BitRate ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForBitRateDesc ()
        {
            string sort = BansheeQuery.GetSort ("BitRate", false);
            AssertAreEquivalent (@"CoreTracks.BitRate DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForBitsPerSampleAsc ()
        {
            string sort = BansheeQuery.GetSort ("BitsPerSample", true);
            AssertAreEquivalent (@"CoreTracks.BitsPerSample ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForBitsPerSampleDesc ()
        {
            string sort = BansheeQuery.GetSort ("BitsPerSample", false);
            AssertAreEquivalent (@"CoreTracks.BitsPerSample DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForCommentAsc ()
        {
            string sort = BansheeQuery.GetSort ("Comment", true);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Comment) ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForCommentDesc ()
        {
            string sort = BansheeQuery.GetSort ("Comment", false);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Comment) DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForComposerAsc ()
        {
            string sort = BansheeQuery.GetSort ("Composer", true);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Composer) ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForComposerDesc ()
        {
            string sort = BansheeQuery.GetSort ("Composer", false);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Composer) DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForConductorAsc ()
        {
            string sort = BansheeQuery.GetSort ("Conductor", true);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Conductor) ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForConductorDesc ()
        {
            string sort = BansheeQuery.GetSort ("Conductor", false);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Conductor) DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDateAddedStampAsc ()
        {
            string sort = BansheeQuery.GetSort ("DateAddedStamp", true);
            AssertAreEquivalent (@"CoreTracks.DateAddedStamp ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDateAddedStampDesc ()
        {
            string sort = BansheeQuery.GetSort ("DateAddedStamp", false);
            AssertAreEquivalent (@"CoreTracks.DateAddedStamp DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDiscAsc ()
        {
            string sort = BansheeQuery.GetSort ("Disc", true);
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDiscDesc ()
        {
            string sort = BansheeQuery.GetSort ("Disc", false);
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc DESC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDiscCountAsc ()
        {
            string sort = BansheeQuery.GetSort ("DiscCount", true);
            AssertAreEquivalent (@"CoreTracks.DiscCount ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDiscCountDesc ()
        {
            string sort = BansheeQuery.GetSort ("DiscCount", false);
            AssertAreEquivalent (@"CoreTracks.DiscCount DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDurationAsc ()
        {
            string sort = BansheeQuery.GetSort ("Duration", true);
            AssertAreEquivalent (@"CoreTracks.Duration ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDurationDesc ()
        {
            string sort = BansheeQuery.GetSort ("Duration", false);
            AssertAreEquivalent (@"CoreTracks.Duration DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForFileSizeAsc ()
        {
            string sort = BansheeQuery.GetSort ("FileSize", true);
            AssertAreEquivalent (@"CoreTracks.FileSize ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForFileSizeDesc ()
        {
            string sort = BansheeQuery.GetSort ("FileSize", false);
            AssertAreEquivalent (@"CoreTracks.FileSize DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForGenreAsc ()
        {
            string sort = BansheeQuery.GetSort ("Genre", true);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Genre) ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForGenreDesc ()
        {
            string sort = BansheeQuery.GetSort ("Genre", false);
            AssertAreEquivalent (@"HYENA_COLLATION_KEY(CoreTracks.Genre) DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForGroupingAsc ()
        {
            string sort = BansheeQuery.GetSort ("Grouping", true);
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForGroupingDesc ()
        {
            string sort = BansheeQuery.GetSort ("Grouping", false);
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber DESC",
                                  sort);
        }

        [Test]
        public void GetSortForLastPlayedAsc ()
        {
            string sort = BansheeQuery.GetSort ("LastPlayed", true);
            AssertAreEquivalent (@"CoreTracks.LastPlayedstamp ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLastPlayedDesc ()
        {
            string sort = BansheeQuery.GetSort ("LastPlayed", false);
            AssertAreEquivalent (@"CoreTracks.LastPlayedstamp DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLastPlayedStampAsc ()
        {
            string sort = BansheeQuery.GetSort ("LastPlayedStamp", true);
            AssertAreEquivalent (@"CoreTracks.LastPlayedStamp ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLastPlayedStampDesc ()
        {
            string sort = BansheeQuery.GetSort ("LastPlayedStamp", false);
            AssertAreEquivalent (@"CoreTracks.LastPlayedStamp DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLastSkippedAsc ()
        {
            string sort = BansheeQuery.GetSort ("LastSkipped", true);
            AssertAreEquivalent (@"CoreTracks.LastSkippedstamp ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLastSkippedDesc ()
        {
            string sort = BansheeQuery.GetSort ("LastSkipped", false);
            AssertAreEquivalent (@"CoreTracks.LastSkippedstamp DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLastSkippedStampAsc ()
        {
            string sort = BansheeQuery.GetSort ("LastSkippedStamp", true);
            AssertAreEquivalent (@"CoreTracks.LastSkippedStamp ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLastSkippedStampDesc ()
        {
            string sort = BansheeQuery.GetSort ("LastSkippedStamp", false);
            AssertAreEquivalent (@"CoreTracks.LastSkippedStamp DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLicenseUriAsc ()
        {
            string sort = BansheeQuery.GetSort ("LicenseUri", true);
            AssertAreEquivalent (@"CoreTracks.LicenseUri ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForLicenseUriDesc ()
        {
            string sort = BansheeQuery.GetSort ("LicenseUri", false);
            AssertAreEquivalent (@"CoreTracks.LicenseUri DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForMimeTypeAsc ()
        {
            string sort = BansheeQuery.GetSort ("MimeType", true);
            AssertAreEquivalent (@"CoreTracks.MimeType ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForMimeTypeDesc ()
        {
            string sort = BansheeQuery.GetSort ("MimeType", false);
            AssertAreEquivalent (@"CoreTracks.MimeType DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForPlayCountAsc ()
        {
            string sort = BansheeQuery.GetSort ("PlayCount", true);
            AssertAreEquivalent (@"CoreTracks.PlayCount ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForPlayCountDesc ()
        {
            string sort = BansheeQuery.GetSort ("PlayCount", false);
            AssertAreEquivalent (@"CoreTracks.PlayCount DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForSkipCountAsc ()
        {
            string sort = BansheeQuery.GetSort ("SkipCount", true);
            AssertAreEquivalent (@"CoreTracks.SkipCount ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForSkipCountDesc ()
        {
            string sort = BansheeQuery.GetSort ("SkipCount", false);
            AssertAreEquivalent (@"CoreTracks.SkipCount DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForScoreAsc ()
        {
            string sort = BansheeQuery.GetSort ("Score", true);
            AssertAreEquivalent (@"CoreTracks.Score ASC,
                                   CoreTracks.Playcount ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForScoreDesc ()
        {
            string sort = BansheeQuery.GetSort ("Score", false);
            AssertAreEquivalent (@"CoreTracks.Score DESC,
                                   CoreTracks.Playcount DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForRandomAsc ()
        {
            string sort = FullTrim (BansheeQuery.GetSort ("Random", true));
            AssertAreEquivalent ("RANDOM ()", sort);
        }

        [Test]
        public void GetSortForRandomDesc ()
        {
            string sort = FullTrim (BansheeQuery.GetSort ("Random", false));
            AssertAreEquivalent ("RANDOM ()", sort);
        }

        [Test]
        public void GetSortForRatingAsc ()
        {
            string sort = BansheeQuery.GetSort ("Rating", true);
            AssertAreEquivalent (@"CoreTracks.Rating ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForRatingDesc ()
        {
            string sort = BansheeQuery.GetSort ("Rating", false);
            AssertAreEquivalent (@"CoreTracks.Rating DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForSampleRateAsc ()
        {
            string sort = BansheeQuery.GetSort ("SampleRate", true);
            AssertAreEquivalent (@"CoreTracks.SampleRate ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForSampleRateDesc ()
        {
            string sort = BansheeQuery.GetSort ("SampleRate", false);
            AssertAreEquivalent (@"CoreTracks.SampleRate DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }


        [Test]
        public void GetSortForTitleAsc ()
        {
            string sort = FullTrim (BansheeQuery.GetSort ("Title", true));
            AssertAreEquivalent (@"CoreTracks.TitleSortKey ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC",
                                 sort);
        }

        [Test]
        public void GetSortForTitleDesc ()
        {
            string sort = FullTrim (BansheeQuery.GetSort ("Title", false));
            AssertAreEquivalent (@"CoreTracks.TitleSortKey DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC",
                                 sort);
        }

        [Test]
        public void GetSortForTrackAsc ()
        {
            string sort = FullTrim (BansheeQuery.GetSort ("Track", true));
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForTrackDesc ()
        {
            string sort = FullTrim (BansheeQuery.GetSort ("Track", false));
            AssertAreEquivalent (@"CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber DESC",
                                 sort);
        }

        [Test]
        public void GetSortForTrackCountAsc ()
        {
            string sort = BansheeQuery.GetSort ("TrackCount", true);
            AssertAreEquivalent (@"CoreTracks.TrackCount ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForTrackCountDesc ()
        {
            string sort = BansheeQuery.GetSort ("TrackCount", false);
            AssertAreEquivalent (@"CoreTracks.TrackCount DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForUriAsc ()
        {
            string sort = BansheeQuery.GetSort ("Uri", true);
            AssertAreEquivalent (@"CoreTracks.Uri ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForUriDesc ()
        {
            string sort = BansheeQuery.GetSort ("Uri", false);
            AssertAreEquivalent (@"CoreTracks.Uri DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForYearAsc ()
        {
            string sort = BansheeQuery.GetSort ("Year", true);
            AssertAreEquivalent (@"CoreTracks.Year ASC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForYearDesc ()
        {
            string sort = BansheeQuery.GetSort ("Year", false);
            AssertAreEquivalent (@"CoreTracks.Year DESC,
                                   CoreAlbums.ArtistNameSortKey ASC,
                                   CoreAlbums.TitleSortKey ASC,
                                   CoreTracks.Disc ASC,
                                   CoreTracks.TrackNumber ASC",
                                 sort);
        }

        [Test]
        public void GetSortForDefault ()
        {
            string sort = BansheeQuery.GetSort ("UnknownField", false);
            Assert.IsNull (sort);
        }

    }
}

#endif
