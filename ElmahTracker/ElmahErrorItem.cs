using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using ElmahTracker.netfx.System.Xml.Linq;

namespace ElmahTracker
{
    public class ElmahErrorItem
    {
        static ElmahErrorItem()
        {
            Initilise();

            ConfigManager.ConfigSave += Initilise;
        }

        private static void Initilise()
        {
            LatestItem = LoadLatestItemFromTheDisk();

            LatestItems = Enumerable.Empty<ElmahErrorItem>();
        }

        public static ElmahErrorItem LatestItem { get; set; }

        public DateTime PubDate { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public int Number { get; set; }

        public string Display
        {
            get { return string.Format("({0}) {1} - {2}", Number, PubDate.ToShortTimeString(), Title); }
        }

        public static IEnumerable<ElmahErrorItem> LatestItems { get; set; }

        public static ElmahErrorItem LoadLatestItemFromTheDisk()
        {
            if (File.Exists(ConfigManager.LatestItemFilePath))
            {
                using (FileStream stream = File.OpenRead(ConfigManager.LatestItemFilePath))
                {
                    try
                    {
                        return new DataContractJsonSerializer(typeof(ElmahErrorItem)).ReadObject(stream) as ElmahErrorItem;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        public static void SaveLatestItemToTheDisk()
        {
            try
            {
                if (!Directory.Exists(ConfigManager.AppDirectory))
                {
                    Directory.CreateDirectory(ConfigManager.AppDirectory);
                }

                using (FileStream stream = File.Create(ConfigManager.LatestItemFilePath))
                {
                    new DataContractJsonSerializer(typeof(ElmahErrorItem)).WriteObject(stream, LatestItem);
                }
            }
            catch { }
        }

        public static int PullItems()
        {
            var list = new LinkedList<ElmahErrorItem>();

            XDocument xdoc = XDocument.Load(ConfigManager.Config.RssUrl);

            dynamic rss = xdoc.Root.ToDynamic();

            dynamic[] items = rss.channel["item"];

            foreach (dynamic item in items)
            {
                string pubDate = item.pubDate;

                list.AddLast(new ElmahErrorItem
                                 {
                                     Title = item.title,
                                     Description = item.description,
                                     PubDate =
                                         DateTime.ParseExact(pubDate, "r", CultureInfo.InvariantCulture,
                                                             DateTimeStyles.AssumeUniversal),
                                     Link = item.link
                                 });
            }

            var newList = (LatestItem != null
                ? list.Where(i => i.PubDate >= LatestItem.PubDate && i.Link != LatestItem.Link)
                : list)
                .ToArray();

            if (newList.Length > 0)
            {
                var latestItems = newList.Concat(LatestItems)
                    .Take(30).ToList();

                for (int i = 0; i < latestItems.Count; i++)
                {
                    latestItems[i].Number = i + 1;
                }

                LatestItems = latestItems;

                LatestItem = LatestItems.First();

                SaveLatestItemToTheDisk();
            }

            return newList.Length;
        }
    }
}