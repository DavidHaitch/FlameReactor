using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlameReactor.DB.Models
{
    public class Flame
    {
        public int ID { get; set; }

        public int? BirthID { get; set; }
        public Breeding? Birth { get; set; }

        public List<Breeding> Breedings { get; } = new List<Breeding>();
        public string DisplayName { get; set; }

        public string Name { get; set; }
        public string GenomePath { get; set; }
        public string Genome { get; set; }
        [JsonIgnore]
        [NotMapped]
        public string GenomePathWeb { get
            {
                return GenomePath.Replace("wwwroot", "");
            } 
        }
        public string ImagePath { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string ImagePathWeb
        {
            get
            {
                return ImagePath.Replace("wwwroot", "");
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string ThumbPath
        {
            get
            {
                if (string.IsNullOrEmpty(ImagePath)) return "";
                return Path.Combine(Path.GetDirectoryName(ImagePath), Path.GetFileNameWithoutExtension(ImagePath) + "_thumb.png");
            }
        }

        [JsonIgnore]
        [NotMapped]
        public bool Named
        {
            get
            {
                return !Unnamed;
            }
        }

        [JsonIgnore]
        [NotMapped]
        public bool Unnamed
        {
            get
            {
                return Name == DisplayName || string.IsNullOrWhiteSpace(DisplayName);
            }
        }

        [JsonIgnore]
        [NotMapped]
        public string ThumbPathWeb
        {
            get
            {
                return ThumbPath.Replace("wwwroot", "");
            }
        }

        public string VideoPath { get; set; }
        
        [JsonIgnore]
        [NotMapped]
        public string VideoPathWeb
        {
            get
            {
                return VideoPath.Replace("wwwroot", "");
            }
        }

        public int Promiscuity { get; set; }
        public int Rating { get; set; }
        public int Generation { get; set; }

        public bool Dead { get; set; }

        public Flame()
        {
            if (DisplayName == null) DisplayName = Name;
        }

        public Flame(string filepath)
        {
            Name = Path.GetFileNameWithoutExtension(filepath);
            if (DisplayName == null) DisplayName = Name;
            GenomePath = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, filepath);
            if (!File.Exists(GenomePath))
            {
                GenomePath = filepath;
            }

            if (!File.Exists(GenomePath))
            {
                throw new FileNotFoundException("Couldn't find " + GenomePath);
            }

            Update();
        }

        public void Update()
        {
            var poolPath = Path.GetDirectoryName(GenomePath);

            ImagePath = Path.Combine(poolPath, Name + ".0.png");
            if (!File.Exists(ImagePath))
            {
                ImagePath = "";
            }

            if(!File.Exists(GenomePath))
            {
                Dead = true;
            }
            else if(string.IsNullOrWhiteSpace(Genome))
            {
                Genome = File.ReadAllText(GenomePath);
            }

            VideoPath = Path.Combine(poolPath, Name, Name + ".mp4");
            if (!File.Exists(VideoPath))
            {
                VideoPath = "";
            }
        }
    }
}
