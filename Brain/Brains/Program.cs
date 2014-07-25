using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace Brains
{
    class Programm
    {
        /// <summary>
        /// 生成神经网络的输入数据
        /// </summary>
        /// <param name="image">位图文件</param>
        /// <returns></returns>
        static List<double> apply_image(Bitmap image)
        {
            List<double> daten = new List<double>();
            int height;//图片高度
            int width;//图片宽度
            int jump = 15;//采样间隔
            lock (image)
            {
                double pixel = image.Height * image.Width;
                height = image.Height;
                width = image.Width;
            }            
            for (int i = 0; i < height - jump; i += jump)
            {
                for (int k = 0; k < width - jump; k += jump)
                {
                    double brightness;
                    lock (image)
                    {
                        brightness = image.GetPixel(k, i).GetBrightness();
                    }
                    //二值化：亮度大于0.5的像素视为1；否则视为0
                    if (brightness > 0.5) { brightness = 1; }
                    else brightness = 0;
                    daten.Add(brightness);
                }
            }
            return daten;
        }
        
        static void Main(string[] args)
        {
            Bitmap A = new Bitmap(@"A.bmp");
            Bitmap B = new Bitmap(@"B.bmp");
            Bitmap C = new Bitmap(@"C.bmp");
            Bitmap T1 = new Bitmap(@"TEST1.bmp");
            Bitmap T2 = new Bitmap(@"TEST2.bmp");
            
            Brain NET = new Brain(100, 3, 0);
            NET.Trainmode = Trainmode.Fast;
            NET.Transferfunction = Transferfunction.Arctan;
            NET.Outputmax = 1;
            
            var inputs = apply_image(A);
            double score = 0;
            Console.WriteLine("训练开始……");
            int count = 0;
            while (NET.Score < 2.9)
            {
                score = 0;

                Console.WriteLine("NET[0]: " + NET[0] + "; NET[1]: " + NET[1]);
                inputs = apply_image(A);
                NET.SetInput(inputs);
                //Console.WriteLine("NET[0]: " + NET[0] + "; NET[1]: " + NET[1]);
                score += NET[0];
                score -= NET[1];
                score -= NET[2];
                //Console.WriteLine("NET[0]: " + NET[0] + "; NET[1]: " + NET[1]);
                inputs = apply_image(B);
                NET.SetInput(inputs);
                score -= NET[0];
                score += NET[1];
                score -= NET[2];
                //Console.WriteLine("NET[0]: " + NET[0] + "; NET[1]: " + NET[1]);
                inputs = apply_image(C);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score += NET[2];

                //Console.Clear();
                Console.WriteLine("第"+NET.Generation.ToString()+"代训练结束！");
                Console.WriteLine("NET[0]: " + NET[0] + "; NET[1]: " + NET[1]);
                //Console.WriteLine(score.ToString("0.000"));
                //Console.WriteLine(count);
                //count++;
                Console.ReadLine();

                NET.Score = score;
                if (Console.KeyAvailable) {
                    //Console.WriteLine("Key is available");
                    NET.StopTraining(); 
                    break;
                }
            }

            //Console.Clear();

            inputs = apply_image(T1);
            NET.SetInput(inputs);
            
            Console.WriteLine("TEST1:");
            Console.WriteLine("A: " + NET.Outputs[0].ToString("0.00"));
            Console.WriteLine("B: " + NET.Outputs[1].ToString("0.00"));
            Console.WriteLine("C: " + NET.Outputs[2].ToString("0.00") + "\n");
            
            int max = NET.Outputs.IndexOf(NET.Outputs.Max());
            switch (max)
            {
                case 0:
                    Console.WriteLine("I SEE A"); break;
                case 1:
                    Console.WriteLine("I SEE B"); break;
                case 2:
                    Console.WriteLine("I SEE C"); break;
            }

            inputs = apply_image(T2);
            NET.SetInput(inputs);

            Console.WriteLine("\n\n\nTEST2:");
            Console.WriteLine("A: " + NET.Outputs[0].ToString("0.00"));
            Console.WriteLine("B: " + NET.Outputs[1].ToString("0.00"));
            Console.WriteLine("C: " + NET.Outputs[2].ToString("0.00") + "\n");


            Console.WriteLine("Drag Picture to console and press enter to try!");
            while (true)
            {
                string path = Console.ReadLine();
                path = path.Remove(0, 1);
                path = path.Remove(path.Length - 1);

                var c = Image.FromFile(path);
                
                try
                {
                    Bitmap user = new Bitmap(@path);
                    Console.Clear();

                    inputs = apply_image(user);
                    NET.SetInput(inputs);                    
                    Console.WriteLine("A: " + NET.Outputs[0].ToString("0.00"));
                    Console.WriteLine("B: " + NET.Outputs[1].ToString("0.00"));
                    Console.WriteLine("C: " + NET.Outputs[2].ToString("0.00") + "\n");
                    max = NET.Outputs.IndexOf(NET.Outputs.Max());
                    switch (max)
                    {
                        case 0:
                            Console.WriteLine("I SEE A"); break;
                        case 1:
                            Console.WriteLine("I SEE B"); break;
                        case 2:
                            Console.WriteLine("I SEE C"); break;
                    }
                    NET.ShowGraph();
                }
                catch { Console.WriteLine("Error reading file"); }
            }

            Console.ReadKey();
        }
    }
}
