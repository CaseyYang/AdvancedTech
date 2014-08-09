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
            Bitmap D = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\D.bmp");
            Bitmap E = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\E.bmp");
            Bitmap F = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\F.bmp");
            Bitmap G = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\G.bmp");
            Bitmap H = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\H.bmp");
            Bitmap I = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\I.bmp");
            Bitmap J = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\J.bmp");
            Bitmap K = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\K.bmp");
            Bitmap L = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\L.bmp");
            Bitmap M = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\M.bmp");
            Bitmap N = new Bitmap(@"E:\Documents\Visual Studio 2013\Projects\AdvancedTech\Brain\Alpha\N.bmp");

            Bitmap T1 = new Bitmap(@"TEST1.bmp");
            Bitmap T2 = new Bitmap(@"TEST2.bmp");
            
            Brain NET = new Brain(100, 14, 0);
            NET.Trainmode = Trainmode.Fast;
            NET.Transferfunction = Transferfunction.Arctan;
            NET.Outputmax = 1;
            
            var inputs = apply_image(A);
            double score = 0;
            Console.WriteLine("训练开始……");
            while (NET.Score < 13.9)
            {
                score = 0;

                inputs = apply_image(A);
                NET.SetInput(inputs);
                score += NET[0];
                for (int i = 1; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(B);
                NET.SetInput(inputs);
                score -= NET[0];
                score += NET[1];
                for (int i = 2; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(C);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score += NET[2];
                for (int i = 3; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(D);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score += NET[3];
                for (int i = 4; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(E);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score += NET[4];
                for (int i = 5; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(F);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score += NET[5];
                for (int i = 6; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(G);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score += NET[6];
                for (int i = 7; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(H);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score -= NET[6];
                score += NET[7];
                for (int i = 8; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(I);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score -= NET[6];
                score -= NET[7];
                score += NET[8];
                for (int i = 9; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(J);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score -= NET[6];
                score -= NET[7];
                score -= NET[8];
                score += NET[9];
                for (int i = 10; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(K);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score -= NET[6];
                score -= NET[7];
                score -= NET[8];
                score -= NET[9];
                score += NET[10];
                for (int i = 11; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(K);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score -= NET[6];
                score -= NET[7];
                score -= NET[8];
                score -= NET[9];
                score -= NET[10];
                score += NET[11];
                for (int i = 12; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(K);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score -= NET[6];
                score -= NET[7];
                score -= NET[8];
                score -= NET[9];
                score -= NET[10];
                score -= NET[11];
                score += NET[12];
                for (int i = 13; i < 14; i++)
                {
                    score -= NET[i];
                }

                inputs = apply_image(K);
                NET.SetInput(inputs);
                score -= NET[0];
                score -= NET[1];
                score -= NET[2];
                score -= NET[3];
                score -= NET[4];
                score -= NET[5];
                score -= NET[6];
                score -= NET[7];
                score -= NET[8];
                score -= NET[9];
                score -= NET[10];
                score -= NET[11];
                score -= NET[12];
                score += NET[13];

                Console.Clear();

                NET.Score = score;
                if (Console.KeyAvailable) {
                    NET.StopTraining(); 
                    break;
                }
            }
            Console.WriteLine("训练结束！");

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
                    Console.Clear();
                    Bitmap user = new Bitmap(@path);
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
                    //画出神经网络结构图
                    //NET.ShowGraph();
                }
                catch { Console.WriteLine("Error reading file"); }
            }
            Console.ReadKey();
        }
    }
}
