using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace MultiIndexCollection.Benchmarks
{
    // [MemoryDiagnoser]
    public class FilteringBenchmark
    {
        public class Product
        {
            public int Id { get; set; }
            public int Code { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        // [Params(100, 300, 1000, 3000, 10000, 30000, 100000, 300000, 1000000)]
        [Params(300, 3000, 30000, 300000)]
        public int Length { get; set; }
        
        private List<Product> _list;
        private IndexedCollection<Product> _indexed;

        private int _maxId = 0;

        private int _maxCode = 0;
        private const int _maxCodeDuplicates = 3;

        private const int _minPrice = 50;
        private const int _maxPrice = 5000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var random = new Random();

            _list = new List<Product>(Length);

            for (int i = 0; i < Length && _list.Count < Length; i++)
            {
                string first = Words[random.Next(Words.Length)];
                string second = Words[random.Next(Words.Length)];

                int duplicatesCount = random.Next(_maxCodeDuplicates) + 1;

                for (int j = 0; j < duplicatesCount; j++)
                {
                    string third = Words[random.Next(Words.Length)];

                    _list.Add(new Product
                    {
                        Id = _maxId++,
                        Code = i,
                        Name = $"{first} {second} {third}",
                        Price = (decimal)random.Next(_minPrice * 20, _maxPrice * 20) / 20,
                    });
                }
                _maxCode = i;
            }
            
            _list.Shuffle();

            _indexed = _list
                .IndexBy(p => p.Id)
                .IndexBy(p => p.Code)
                .IndexBy(p => p.Name, true)
                .IndexBy(p => p.Price, true);
        }

        private int _id = 0;

        [Benchmark]
        public Product LinqFirstOrDefault()
        {
            _id = (_id + 1) % _maxId;
            return _list.FirstOrDefault(p => p.Id == _id);
        }

        [Benchmark]
        public Product IndexedFirstOrDefault()
        {
            _id = (_id + 1) % _maxId;
            return _indexed.FirstOrDefault(p => p.Id == _id);
        }

        private int _code = 0;

        [Benchmark]
        public Product LinqExactMatch()
        {
            _code = (_code + 1) % _maxCode;

            return _list.Where(p => p.Code == _code).Enumerate();
        }
        
        [Benchmark]
        public Product IndexedExactMatch()
        {
            _code = (_code + 1) % _maxCode;

            return _indexed.Where(p => p.Code == _code).Enumerate();
        }

        int _pricePercentage = 0;

        [Benchmark]
        public Product LinqRange()
        {
            _pricePercentage = (_pricePercentage + 1) % 100;

            decimal onePercent = (decimal)(_maxPrice - _minPrice) / 100;

            decimal from = _minPrice + _pricePercentage * onePercent;

            decimal to = from + onePercent;
            
            return _list.Where(p => p.Price >= from && p.Price <= to).Enumerate();
        }

        [Benchmark]
        public Product IndexedRange()
        {
            _pricePercentage = (_pricePercentage + 1) % 100;

            decimal onePercent = (decimal)(_maxPrice - _minPrice) / 100;

            decimal from = _minPrice + _pricePercentage * onePercent;

            decimal to = from + onePercent;

            return _indexed.Where(p => p.Price >= from && p.Price <= to).Enumerate();
        }

        int _wordIndex = 0;

        [Benchmark]
        public Product LinqStartsWith()
        {
            _wordIndex = (_wordIndex + 1) % Words.Length;

            string word = Words[_wordIndex];
            
            return _list.Where(p => p.Name.StartsWith(word)).Enumerate();
        }

        [Benchmark]
        public Product IndexedStartsWith()
        {
            _wordIndex = (_wordIndex + 1) % Words.Length;

            string word = Words[_wordIndex];
            
            return _indexed.Where(p => p.Name.StartsWith(word)).Enumerate();
        }

        #region Words

        private static readonly string[] Words = new[]
        {
            "people","history","way","art","world","information","map","family","government",
            "health","system","computer","meat","year","thanks","music","person","reading","method",
            "data","food","understanding","theory","law","bird","literature","problem","software",
            "control","knowledge","power","ability","economics","love","internet","television",
            "science","library","nature","fact","product","idea","temperature","investment","area",
            "society","activity","story","industry","media","thing","oven","community","definition",
            "safety","quality","development","language","management","player","variety","video",
            "week","security","country","exam","movie","organization","equipment","physics",
            "analysis","policy","series","thought","basis","boyfriend","direction","strategy",
            "technology","army","camera","freedom","paper","environment","child","instance","month",
            "truth","marketing","university","writing","article","department","difference","goal",
            "news","audience","fishing","growth","income","marriage","user","combination","failure",
            "meaning","medicine","philosophy","teacher","communication","night","chemistry",
            "disease","disk","energy","nation","road","role","soup","advertising","location",
            "success","addition","apartment","education","math","moment","painting","politics",
            "attention","decision","event","property","shopping","student","wood","competition",
            "distribution","entertainment","office","population","president","unit","category",
            "cigarette","context","introduction","opportunity","performance","driver","flight",
            "length","magazine","newspaper","relationship","teaching","cell","dealer","finding",
            "lake","member","message","phone","scene","appearance","association","concept",
            "customer","death","discussion","housing","inflation","insurance","mood","woman",
            "advice","blood","effort","expression","importance","opinion","payment","reality",
            "responsibility","situation","skill","statement","wealth","application","city","county",
            "depth","estate","foundation","grandmother","heart","perspective","photo","recipe",
            "studio", "topic","collection","depression","imagination","passion","percentage",
            "resource","setting","ad","agency","college","connection","criticism","debt","description",
            "memory","patience","secretary","solution","administration","aspect","attitude","director",
            "personality","psychology","recommendation","response","selection","storage","version",
            "alcohol","argument","complaint","contract","emphasis","highway","loss","membership",
            "possession","preparation","steak","union","agreement","cancer","currency","employment",
            "engineering","entry","interaction","mixture","preference","region","republic","tradition",
            "virus","actor","classroom","delivery","device","difficulty","drama","election","engine",
            "football","guidance","hotel","owner","priority","protection","suggestion","tension",
            "variation","anxiety","atmosphere","awareness","bath","bread","candidate","climate",
            "comparison","confusion","construction","elevator","emotion","employee","employer","guest",
            "height","leadership","mall","manager","operation","recording","sample","transportation",
            "charity","cousin","disaster","editor","efficiency","excitement",
        };

        #endregion
    }
}
