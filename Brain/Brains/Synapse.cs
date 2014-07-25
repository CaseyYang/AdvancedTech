using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace Brains
{
    public static class StaticHelp
    {
        /// <summary>
        ///给定集合和要比较的成员，使用该集合中元素类型的默认排序顺序比较器，找出集合中的最大值
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, Comparer<TKey>.Default);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            if (comparer == null) throw new ArgumentNullException("comparer");
            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }


        }


        static int seed = Environment.TickCount;

        static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));//这里用ThreadLocal有必要吗？

        public static Random Rand
        {
            get { return random.Value; }
            private set { }
        }

        /// <summary>
        /// 把集合values填充入动态集合x中
        /// </summary>
        /// <param name="x"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static ObservableCollection<double> SetRange(this ObservableCollection<double> x, List<double> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                x[i] = (double)values[i];
            }
            return x;
        }

        /// <summary>
        /// 序列化相关？
        /// </summary>
        /// <param name="Toserialize"></param>
        /// <returns></returns>
        public static byte[] Serialize(object Toserialize)
        {
            if (Toserialize == null) return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, Toserialize);

            return ms.ToArray();
        }

        /// <summary>
        /// 反序列化相关
        /// </summary>
        /// <param name="Serializedcontrol"></param>
        /// <returns></returns>
        public static object Deserialize(byte[] Serializedcontrol)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(Serializedcontrol, 0, Serializedcontrol.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var child = binForm.Deserialize(memStream);

            return child;
        }
    }
    /// <summary>
    /// 训练模式：Quanity模式每代需要16个训练集；Fast模式每代只需一个训练集
    /// </summary>
    public enum Trainmode
    {
        [Description("Needs 16 training sets per generation, tries everything to find a global optimum")]
        Quality,
        [Description("Every training set is a generation, may get stuck in a local minimum.")]
        Fast
    }
    /// <summary>
    /// 激励函数类型：Arctan表示反正切函数；Stepfunction表示0-1阶跃激励函数
    /// </summary>
    public enum Transferfunction
    {
        Arctan,
        Stepfunction
    }

    [Serializable()]
    public class Brain
    {
        #region 一般成员变量
        public Transferfunction Transferfunction = Transferfunction.Arctan;
        /// <summary>
        /// Allows training for neuronal network. Apply your data and finally set score. Neuronal net tries to maximise your score!
        /// </summary>
        public Trainmode Trainmode = Trainmode.Fast;

        public string Name = "Neuronal net";
        public double Outputmax = 1.0;// Sets what the Outputs of the network should output as a maximum
        Random r
        {
            get { return StaticHelp.Rand; }
            set { }
        }
        #endregion

        #region 关键成员变量
        /// <summary>
        /// 神经元集合
        /// </summary>
        List<List<Neuron>> Layer = new List<List<Neuron>>();
        #region Inputs
        List<Neuron> _inputs = new List<Neuron>();
        public ObservableCollection<double> Inputs
        {
            get;
            private set;
        }
        /// <summary>
        /// Inputs the List to the neuronal network must be castable to double
        /// </summary>
        /// <param name="data"> Raw data to be feeded into the network </param>
        public void SetInput<T>(List<T> data)
        {
            if (data.Count > Inputs.Count)
            {
                throw new Exception("List to feed is bigger than neuronal inputlayer");
            }
            Inputs.SetRange(data.ConvertAll<double>(x => Convert.ToDouble(x)));
        }
        void Inputs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
            {
                throw new Exception("Cannot change inputlayer size of existing neuronal network! , use setrange");
            }
            if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                answer.Clear();//当input集合元素有变动时，清空原有答案
                if (e.NewItems.Count != 1)
                {
                    throw new Exception("number of elements changed!=1");
                }
                _inputs[e.NewStartingIndex].output = (double)e.NewItems[0];//输入层中神经元的输出值
            }
        }
        #endregion
        #region Outputs
        List<Neuron> _outputs = new List<Neuron>();
        ObservableCollection<double> outputs;
        public ObservableCollection<double> Outputs
        {
            get
            {
                return new ObservableCollection<double>(_outputs.ConvertAll<double>(x => (double)x));
            }
            private set { outputs = value; }
        }
        void Outputs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new Exception("Cannot change outputlayer of existing neuronal network!");
        }
        #endregion
        /// <summary>
        /// 输出分数较大的神经网络结构
        /// </summary>
        List<Genome> best = new List<Genome>();
        /// <summary>
        /// 当前输出的最大分数
        /// </summary>
        double score = double.NegativeInfinity;
        /// <summary>
        /// 所有神经元的名字和输出值组成的键值对集合
        /// </summary>
        Dictionary<int, double> answer;
        /// <summary>
        /// 有效训练次数
        /// </summary>
        int generation = 0;
        /// <summary>
        /// 迭代次数
        /// </summary>
        public int Generation
        {
            get { return generation; }
            private set { generation = value; }
        }

        /// <summary>
        /// Evolutionary fitness, tries to achieve a better score next time. Each generation is better than the before.
        /// </summary>
        public double Score
        {
            get { return score; }
            set
            {
                if (Trainmode == Trainmode.Quality)
                {
                    if (best.Count == 0) { best.Add(new Genome(this)); }
                    var max = best.MaxBy(x => x.score);
                    if (value > max.score)
                    {
                        Generation++;
                    }
                    score = value;
                    //如果有效训练代数为0且能得到较好分数的神经网络不到10个时，不加选择地保存当前的神经网络
                    //如果能得到较好分数的神经网络不到5个
                    if (Generation == 0 && best.Count < 10)
                    {
                        best.Add(new Genome(this));//保存当前神经网络的结构
                        this.random();//重新随机生成每个神经元的输入权重
                    }
                    else if (best.Count < 5)//
                    {
                        best.Add(new Genome(this));
                        max.SetGenesTo(this);
                        this.mutate();
                    }
                    else
                    {
                        max.SetGenesTo(this);
                        best.Clear();
                    }
                }
                if (Trainmode == Trainmode.Fast)
                {
                    if (best.Count == 0) { best.Add(new Genome(this)); }
                    var max = best.MaxBy(x => x.score);
                    if (value > max.score)//如果得到了比当前最优结果更好的分数
                    {
                        score = value;//更新分数
                        Generation++;//增加代数
                        best.Clear();
                        best.Add(new Genome(this));//保存当前最优结果的神经网络结构
                    }
                    else
                    {
                        max.SetGenesTo(this);//当前的神经网络不如最优神经网络，因此把当前神经网络替换成最优神经网络
                        this.mutate();
                    }
                }
            }
        }
        #endregion

        #region 构造函数和初始化
        /// <summary>
        /// Generates basic neuronal net with 1 hidden layer, size between inputs and outputs
        /// </summary>
        /// <param name="inputs">Nr of inputs of network (get with Brain.inputs) </param>
        /// <param name="outputs">Nr of outputs of network (get with Brain.outputs) </param>
        public Brain(int inputs, int outputs)
        {
            List<int> widths = new List<int>();
            widths.Add((outputs + inputs) / 2);

            Init(inputs, outputs, 1, widths.ToArray());
        }
        /// <summary>
        /// Generates basic neuronal net with n hidden layers, size of layers = outputs or inputs (bigger one)
        /// </summary>
        /// <param name="inputs">Nr of inputs of network (get with Brain.inputs) </param>
        /// <param name="outputs">Nr of outputs of network (get with Brain.outputs) or brain[] </param>
        /// <param name="layers">Nr of hidden layers of neuronal net</param>
        public Brain(int inputs, int outputs, int layers)
        {

            List<int> widths = new List<int>();

            for (int i = 0; i < layers; i++)
            {
                widths.Add(Math.Max(inputs, outputs));
            }

            Init(inputs, outputs, layers, widths.ToArray());
        }
        /// <summary>
        /// /// <summary>
        /// Generates advanced neuronal net with all parameters explicit
        /// </summary>
        /// <param name="inputs">Nr of inputs of network (get with Brain.inputs) </param>
        /// <param name="outputs">Nr of outputs of network (get with Brain.outputs) </param>
        /// <param name="sizes">Width of each layer
        public Brain(int inputs, int outputs, int[] sizes)
        {
            Init(inputs, outputs, sizes.Count(), sizes);
        }
        ///<summary>
        ///Creates new Brain with inputs,layer,layersize,outputs
        ///</summary>
        void Init(int inputs, int outputs, int layers, int[] sizes)
        {
            //构造输入层和输出层容器
            var incount = new double[inputs].ToList();
            var outcount = new double[outputs].ToList();
            Inputs = new ObservableCollection<double>(incount);
            Inputs.CollectionChanged += Inputs_CollectionChanged;
            Outputs = new ObservableCollection<double>(outcount);
            Outputs.CollectionChanged += Outputs_CollectionChanged;
            answer = new Dictionary<int, double>();
            r = new Random();
            int nr = 0;//nr是神经元的名字。对于一个神经网络而言，其名字是唯一的
            //构造输入层神经元
            for (int i = 0; i < inputs; i++)
            {
                _inputs.Add(new Neuron(this, nr));
                _inputs[i].output = Inputs[i];//这句话的作用是什么？动态绑定？
                nr++;
            }
            //构造隐藏层神经元
            for (int i = 0; i < layers; i++)
            {
                List<Neuron> Part = new List<Neuron>();
                for (int z = 0; z < sizes[i]; z++)
                {
                    if (i == 0) { Part.Add(new Neuron(this, _inputs, nr)); nr++; }
                    else { Part.Add(new Neuron(this, Layer[i - 1], nr)); nr++; }
                }
                Layer.Add(Part);
            }
            //构造输出层神经元
            for (int i = 0; i < outputs; i++)
            {
                if (Layer.Count == 0) { _outputs.Add(new Neuron(this, _inputs, nr)); nr++; }
                else { _outputs.Add(new Neuron(this, Layer[Layer.Count - 1], nr)); nr++; }
            }
        }
        #endregion

        #region 一些成员函数
        /// <summary>
        /// 对Brain类型重载[]操作，返回_outputs集合中给定下标索引的元素
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public double this[int output]
        {
            get { return _outputs[output].output; }
            private set { }

        }
        /// <summary>
        /// 对神经网络中的所有神经元（包括输出层）的权值进行初始化
        /// </summary>
        private void random()
        {
            foreach (List<Neuron> neurons in Layer)
            {
                foreach (Neuron currentNeuron in neurons)
                {
                    currentNeuron.seed();
                }
            }
            foreach (Neuron currentNeuron in _outputs)
            {
                currentNeuron.seed();
            }
        }
        /// <summary>
        /// 对神经网络中的所有神经元（包括输出层）的权值进行突变
        /// 突变方式：
        /// </summary>
        private void mutate()
        {
            foreach (List<Neuron> neurons in Layer)
            {
                foreach (Neuron currentNeuron in neurons)
                {
                    currentNeuron.mutate();
                }
            }
            foreach (Neuron currentNeuron in _outputs)
            {
                currentNeuron.mutate();
            }
        }
        /// <summary>
        /// 序列化整个神经网络
        /// </summary>
        public byte[] Serialize()
        {
            return StaticHelp.Serialize(this);
        }
        public void StopTraining()
        {
            if (best.Count != 0)
            {
                var max = best.MaxBy(x => x.score);
                max.SetGenesTo(this);
            }
        }
        #endregion

        #region 使用序列化/反序列化机制深拷贝一个对象
        public static T Copy<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException("Object cannot be null");
            return (T)Deserialize(Serialize(obj));
        }
        public static byte[] Serialize(object Toserialize)
        {
            if (Toserialize == null) return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, Toserialize);

            return ms.ToArray();
        }
        public static object Deserialize(byte[] Serializedcontrol)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(Serializedcontrol, 0, Serializedcontrol.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            var child = binForm.Deserialize(memStream);

            return child;
        }
        #endregion

        #region 可视化相关
        void drawnode(Graphics g, int X, int Y, double value)
        {
            g.DrawEllipse(Pens.Black, X, Y, 30, 30);
            g.DrawString(value.ToString("0.00"), new Font("Arial", 7), Brushes.Red, X + 5, Y + 10);
        }
        void connectnode(Graphics g, int X, int Y, int nrlastlayer, Point startlastlayer)
        {

            for (int i = 0; i < nrlastlayer; i++)
            {
                g.DrawLine(new Pen(Color.FromArgb(200, Color.Blue)), X, Y + 15, startlastlayer.X + 30, startlastlayer.Y + 15 + i * 35);
            }
        }
        /// <summary>
        /// 显示神经网络的结构图 (async)
        /// </summary>
        public void ShowGraph()
        {
            Task.Factory.StartNew(() =>
            {
                Form wnd = new Form();
                Bitmap image = this.ToBitmap();
                image.Save("network.bmp");

                wnd.Size = image.Size;
                wnd.Controls.Add(new PictureBox()
                {
                    Image = image,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Dock = DockStyle.Fill
                });

                wnd.ShowDialog();
            });
        }

        public Bitmap ToBitmap()
        {
            int layercount = Layer.Count();

            int nodewidth = 190;
            int nodeheight = 35;


            int picwidth = nodewidth * 2 + layercount * nodewidth + 30;

            int inputs = _inputs.Count;
            int outputs = _outputs.Count;


            int layers = 0;
            if (Layer.Count != 0) { layers = Layer.Max(y => y.Count); }

            int maxheight = Math.Max(Math.Max(inputs, outputs), layers);

            int picheight = nodeheight * maxheight;


            Bitmap pic = new Bitmap(picwidth, picheight);
            Graphics g = Graphics.FromImage(pic);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            #region inputs
            int inheight = inputs * nodeheight;
            Point startpos = new Point(30, picheight / 2 - inheight / 2);
            g.Clear(Color.Transparent);
            for (int i = 0; i < inputs; i++)
            {
                drawnode(g, startpos.X, startpos.Y + i * nodeheight, _inputs[i].output);
                g.DrawString(Inputs[i].ToString("0.00"), new Font("Arial", 7), Brushes.Black, 5, startpos.Y + 10 + i * nodeheight);
            }
            #endregion

            #region layers
            for (int u = 0; u < layercount; u++)
            {
                inheight = Layer[u].Count * nodeheight;
                startpos = new Point(30 + nodewidth * (1 + u), picheight / 2 - inheight / 2);

                for (int i = 0; i < Layer[u].Count; i++)
                {
                    drawnode(g, startpos.X, startpos.Y + i * nodeheight, Layer[u][i].output);


                    if (u == 0)
                    {
                        Point laststart = new Point(30, picheight / 2 - inputs * nodeheight / 2);
                        int lastcount = inputs;
                        connectnode(g, startpos.X, startpos.Y + i * nodeheight, lastcount, laststart);
                    }
                    else
                    {
                        Point laststart = new Point(30 + nodewidth * (1 + u - 1), picheight / 2 - Layer[u - 1].Count * nodeheight / 2);
                        int lastcount = Layer[u - 1].Count;
                        connectnode(g, startpos.X, startpos.Y + i * nodeheight, lastcount, laststart);
                    }
                }
            }
            #endregion

            #region end

            Point endpos = new Point(30 + (layercount + 1) * nodewidth, picheight / 2 - outputs * nodeheight / 2);
            for (int i = 0; i < outputs; i++)
            {

                drawnode(g, endpos.X, endpos.Y + i * nodeheight, _outputs[i].output);
                g.DrawString(_outputs[i].output.ToString("0.00"), new Font("Arial", 7), Brushes.Black, endpos.X + 35, endpos.Y + i * nodeheight + 10);

                if (layers == 0)
                {
                    connectnode(g, endpos.X, endpos.Y + i * nodeheight, _inputs.Count(), new Point(30 + layercount * nodewidth, picheight / 2 - _inputs.Count() * nodeheight / 2));
                }
                else
                {
                    connectnode(g, endpos.X, endpos.Y + i * nodeheight, Layer.Last().Count, new Point(30 + layercount * nodewidth, picheight / 2 - Layer.Last().Count * nodeheight / 2));
                }
            }

            #endregion


            return pic;
        }
        #endregion

        [Serializable()]
        /// <summary>
        /// Helper class for faster serialization (no recursion and no references)
        /// Genome保存的是神经网络的基本结构：包括隐藏层和输出层的层数；每个神经元的输入权值等
        /// </summary>
        class Genome
        {
            /// <summary>
            /// 神经网络名称
            /// </summary>
            public string name;
            /// <summary>
            /// 神经网络代数
            /// </summary>
            public int generation;
            public double score;
            /// <summary>
            /// 神经网络的层数
            /// </summary>
            int layercount;
            /// <summary>
            /// 每层神经元个数
            /// </summary>
            int[] layersizes;
            /// <summary>
            /// 输出层神经元个数
            /// </summary>
            int outputcount;
            /// <summary>
            /// 所有神经元（包括输出层）的输入权值
            /// </summary>
            public List<double> genes = new List<double>();

            /// <summary>
            /// Generates lightweight genome for instant serialization
            /// </summary>
            public Genome(Brain tocopy)
            {
                name = tocopy.Name;
                generation = tocopy.generation;
                score = tocopy.score;
                layercount = tocopy.Layer.Count;
                List<int> sizes = new List<int>();
                tocopy.Layer.ForEach(x => sizes.Add(x.Count));
                layersizes = sizes.ToArray();
                outputcount = tocopy.Outputs.Count;
                for (int layer = 0; layer < layercount; layer++)
                {
                    for (int neuron = 0; neuron < layersizes[layer]; neuron++)
                    {
                        genes.AddRange(tocopy.Layer[layer][neuron].genome);
                    }
                }
                for (int neuron = 0; neuron < outputcount; neuron++)
                {
                    genes.AddRange(tocopy._outputs[neuron].genome);
                }
            }
            /// <summary>
            /// 把该Genome实例保存的神经网络参数赋给一个神经网络
            /// </summary>
            /// <param name="Oldbrain"></param>
            public void SetGenesTo(Brain Oldbrain)
            {
                if (Oldbrain.answer != null) Oldbrain.answer.Clear();
                var unlinked = Copy(this);
                Oldbrain.Name = unlinked.name;
                Oldbrain.generation = unlinked.generation;
                Oldbrain.score = unlinked.score;
                int index = 0;
                for (int layer = 0; layer < layercount; layer++)
                {
                    for (int neuron = 0; neuron < layersizes[layer]; neuron++)
                    {
                        int genecount = Oldbrain.Layer[layer][neuron].genome.Count;
                        Oldbrain.Layer[layer][neuron].genome = unlinked.genes.GetRange(index, genecount);
                        index += genecount;
                    }
                }
                for (int neuron = 0; neuron < outputcount; neuron++)
                {
                    int genecount = Oldbrain._outputs[neuron].genome.Count;
                    Oldbrain._outputs[neuron].genome = unlinked.genes.GetRange(index, genecount);
                    index += genecount;
                }
            }
        }

        [Serializable()]
        class Neuron
        {
            #region 成员变量
            /// <summary>
            /// 该神经元所属的神经网络
            /// </summary>
            Brain Parent;
            int _name;
            public int name
            {
                get
                {
                    return _name;
                }
                set
                {
                    if (Parent != null && Parent.answer != null) Parent.answer.Clear();
                    _name = value;
                }
            }
            double ret = double.PositiveInfinity;
            /// <summary>
            /// 所有指向该神经元的神经元集合
            /// </summary>
            public List<Neuron> previous;
            /// <summary>
            /// 神经元的输入权值
            /// </summary>
            public List<double> genome = new List<double>();
            /// <summary>
            /// 该神经元的输出
            /// </summary>
            public double output
            {
                get
                {
                    //如果ret不是正无穷（默认初始值），说明该神经元要么是输入层神经元，被SetInput函数访问过；要么是在计算其他神经元的输入值时该神经元的输出值已被计算过
                    if (!double.IsInfinity(ret))
                    {
                        return ret;
                    }
                    else
                    {
                        double outp;
                        //计算该神经元的输出值；如果神经网络的answer集合中有该神经元的name，则说明在计算其他神经元的输入值时该神经元的输出值已被计算过，直接返回outp即可
                        if (!Parent.answer.ContainsKey(name))
                        {
                            outp = sum();
                            lock (Parent)
                            {
                                Parent.answer.Add(name, outp);
                            }
                            return outp;
                        }
                        else
                        {
                            Parent.answer.TryGetValue(name, out outp);
                            return outp;
                        }
                    }
                }
                set
                {
                    ret = value;
                }
            }
            #endregion

            #region 构造函数
            /// <summary>
            /// 神经元构造函数
            /// </summary>
            /// <param name="camefrom">神经元所在的神经网络</param>
            /// <param name="setname">神经网络名称</param>
            public Neuron(Brain camefrom, int setname)
            {
                name = setname;
                genome = new List<double>();
            }
            /// <summary>
            /// 神经元构造函数
            /// </summary>
            /// <param name="camefrom">神经元所在的神经网络</param>
            /// <param name="backlayer">指向该神经元的神经元集合，一般是神经网络中的前一层神经元</param>
            /// <param name="setname">神经网络名称</param>
            public Neuron(Brain camefrom, List<Neuron> backlayer, int setname)
            {
                Parent = camefrom;
                name = setname;
                genome = new List<double>();
                //给每个输入权重随机赋值
                for (int i = 0; i <= backlayer.Count; i++)
                {
                    genome.Add(between(-4, 4));
                    //genome.Add(1);
                }
                //此处即所谓的“激励值权重”，但为什么是0呢？
                genome[backlayer.Count] = 0;
                previous = backlayer;
            }
            #endregion

            #region 神经元核心函数
            /// <returns></returns>
            /// <summary>
            /// 给每个输入权重随机赋值
            /// </summary>
            public void seed()
            {
                for (int i = 0; i < genome.Count; i++)
                {
                    genome[i] = between(-4, 4);
                }
            }
            /// <summary>
            /// 突变函数：修改所有输入权重
            /// </summary>
            public void mutate()
            {
                Parent.answer.Clear();
                for (int i = 0; i < genome.Count; i++)
                {
                    if (percentage(2)) { genome[i] = between(-4, 4); }
                    if (percentage(10)) { genome[i] += between(-0.1, 0.1); }

                    if (genome[i] > 4) { genome[i] = between(-4, 4); }
                    if (genome[i] < -4) { genome[i] = between(-4, 4); }
                }
            }
            /// <summary>
            /// 求得该神经元的输出值
            /// </summary>
            double sum()
            {
                //sum and tranferfunction
                double back = 0;
                for (int i = 0; i < previous.Count; i++)
                {
                    back += previous[i].output * genome[i];
                }
                if (Parent.Transferfunction == Transferfunction.Stepfunction)
                {
                    back = back / previous.Count;
                }
                back = transfer(back);
                return back;
            }
            /// <summary>
            /// 激励函数
            /// </summary>
            /// <param name="invar"></param>
            /// <returns></returns>
            double transfer(double invar)
            {
                if (this.Parent.Transferfunction == Transferfunction.Arctan)
                {
                    return (Math.Atan(invar) / Math.PI + 0.5) * this.Parent.Outputmax;
                }
                else
                {
                    if (invar > 0) return 1;
                    return 0;
                }
            }
            #endregion

            #region 辅助函数
            /// <summary>
            /// 把Neuron类型转为double类型
            /// </summary>
            /// <param name="instance"></param>
            /// <returns></returns>
            public static explicit operator double(Neuron instance)
            {
                return instance.output;
            }
            /// <summary>
            /// 随机返回一个处于给定上下界内的浮点数
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <returns></returns>
            double between(double from, double to)
            {
                int a = Parent.r.Next(1000000000);
                double middle = (to - from) * a / 1000000000.0;
                return middle - Math.Abs(from);
            }
            /// <summary>
            /// 以给定概率随机返回true或false
            /// </summary>
            /// <param name="percent"></param>
            /// <returns></returns>
            bool percentage(double percent)
            {
                double a = Parent.r.Next(10000) / 100.0;//得到一个0~100间的随机浮点数
                //若该数小于给定阈值，返回true；否则返回false
                if (a < percent) { return true; }
                else return false;
            }
            #endregion

        }
    }
    public class HCloner
    {
        public static T DeepCopy_compatible<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null");
            return (T)Process(obj);
        }

        static object Process(object obj)
        {
            if (obj == null)
                return null;
            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            else if (type.IsArray)
            {
                Type elementType = Type.GetType(
                     type.FullName.Replace("[]", string.Empty));
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(Process(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }
            else if (type.IsClass)
            {
                object toret = Activator.CreateInstance(obj.GetType());
                FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    field.SetValue(toret, Process(fieldValue));
                }
                return toret;
            }
            else
                throw new ArgumentException("Unknown type");
        }

    }
}
