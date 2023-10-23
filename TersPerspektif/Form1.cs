using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace TersPerspektif
{
    public partial class Form1 : Form
    {
        public static int resolution = 4000;

        public Form1()
        {
            InitializeComponent();
        }

        double[,] Rt = new double[3, 3];  //ters transformasyon matrisi
        const double PI = 3.1415;
        int xa, ya, za;
        double alfa, beta, teta, xs, zs, D;

        private OpenFileDialog DosyaAcDiyalogu = new OpenFileDialog();
        Bitmap geciciBitmap = null; // Bitmap nesnesi tanýmlanýr
        Bitmap bitmap1; // Bitmap nesnesi tanýmlanýr
        Image res1;


        //---------------------------------------------------------------------------
        // Bu fonksiyon ters transformasyon matrisinin deðerlerini setler.
        private void ttransformasyon(double alfa, double beta, double teta)
        {
            

            Rt[0,0] = Math.Cos(beta * PI / 180) * Math.Cos(teta * PI / 180) + Math.Sin(beta * PI / 180) * Math.Sin(alfa * PI / 180) * Math.Sin(teta * PI / 180);
            Rt[0,1] = -(Math.Cos(beta * PI / 180) * Math.Sin(teta * PI / 180) + Math.Sin(beta * PI / 180) * Math.Sin(alfa * PI / 180) * Math.Cos(teta * PI / 180));
            Rt[0,2] = Math.Sin(beta * PI / 180) * Math.Cos(alfa * PI / 180);

            Rt[1,0] = Math.Cos(alfa * PI / 180) * Math.Sin(teta * PI / 180);
            Rt[1,1] = Math.Cos(alfa * PI / 180) * Math.Cos(teta * PI / 180);
            Rt[1,2] = Math.Sin(alfa * PI / 180) * (-1);

            Rt[2,0] = -(Math.Sin(beta * PI / 180) * Math.Cos(teta * PI / 180) + Math.Cos(beta * PI / 180) * Math.Sin(alfa * PI / 180) * Math.Sin(teta * PI / 180));
            Rt[2,1] = Math.Sin(beta * PI / 180) * Math.Sin(teta * PI / 180) + Math.Cos(beta * PI / 180) * Math.Sin(alfa * PI / 180) * Math.Cos(teta * PI / 180);
            Rt[2,2] = Math.Cos(beta * PI / 180) * Math.Cos(alfa * PI / 180);

          
        }


        //---------------------------------------------------------------------------
        //Bu fonksiyon kullanýcýdan resim seçmesini ister ve belleðe yükler.
        private void button1_Click(object sender, EventArgs e)
        {
                        // Save file dialog

                        
            DosyaAcDiyalogu.Filter = "Image files (*.bmp)|*.bmp|All files (*.*)|*.*";
            if (DosyaAcDiyalogu.ShowDialog() == DialogResult.OK)
            {
                Image Resim1 = Image.FromFile(DosyaAcDiyalogu.FileName);
 

                geciciBitmap = (Bitmap)Resim1.Clone();  //dosyadan okunan image nesnesi bitmap nesnesine dönüþtürülür
                
                pictureBox2.Image = Resim1;

            }
        }


        //---------------------------------------------------------------------------
        //Bu fonksiyon girilen parametrelere göre 400x400'lük bir alana ters perspektif dönüþüm ile doku kaplar.
        private void button2_Click(object sender, EventArgs e)
        {
            int i, j, f1, f2, f3, f4;
            double X, Y, Z, K, payda, den1, den2;

            //Eðer resim henüz seçilmediyse kullanýcýyý uyar.
            if (String.IsNullOrEmpty(DosyaAcDiyalogu.FileName))
            {
                MessageBox.Show("Resim Seçilmedi.");
                
                return;
            }


            //Kullanýcýdan gözlemci koordinat sisteminin merkezini alýr.
            xa = int.Parse(textBox1.Text);
            ya = int.Parse(textBox2.Text);
            za = int.Parse(textBox3.Text);
            //Kullanýcýdan rotasyonlarý alýr.
            alfa = int.Parse(textBox4.Text);
            beta = int.Parse(textBox5.Text);
            teta = int.Parse(textBox6.Text);


            D = ya; // projeksiyon düzleminin merkezden uzakligi
            

            //ters transformasyon matrisini oluþtur.
            ttransformasyon(alfa, beta, teta);

            bitmap1 = new Bitmap(resolution, resolution); ;
            //ekran üzerinde 400x400'lük bir alan tara (xs-zs koordinatlarý).
            for (j = 0; j < resolution; j++)
            {
                for (i = 0; i < resolution; i++)
                {
                    //koordinatlarý merkeze taþý.
                    xs = i - (resolution / 2);
                    zs = j - (resolution / 2);
                    payda = Rt[2,0] * xs + D * Rt[2,1] + Rt[2,2] * zs;
                    if (payda == 0) //eðer ufuk çizgisinde isek K sonsuz çýkar. Bu noktayý atlamalýyýz.
                        continue;
                    K = (-za / payda); //Z=0 yüzeyi ele alýnmýþ.
                    //Bulunan K deðerine göre X ve Y koordinatlarý hesaplanýyor.
                    X = xa + K * (Rt[0,0] * xs + D * Rt[0,1] + zs * Rt[0,2]);
                    Y = ya + K * (Rt[1,0] * xs + D * Rt[1,1] + zs * Rt[1,2]);

                    //Zemin olduðunu belirtmek için...
                    if (payda > 0)
                    { 
                        // X ve Y yi en yakýn deðere yuvarla.
                        f1 = (int)Math.Floor(X + 0.5);
                        f3 = (int)Math.Floor(Y + 0.5);
                        // mod iþlemi yardýmýyla dokuyu kapla.
                        f2 = geciciBitmap.Width;
                        f4 = geciciBitmap.Height;
                        den1 =Math.Abs(f1 % f2);
                        den2 =Math.Abs(f3 % f4);
                        //Eðer 0 noktasýndan gerisini görüyorsak görüntünün bütünlüðü açýsýndan ters alma yapmalýyýz.
                        if (X < 0)
                            den1 = geciciBitmap.Width - den1 - 1;
                        if (Y < 0)
                            den2 = (int)(geciciBitmap.Height - den2) % geciciBitmap.Height;
                        //Ýlgili doku deðerini ekrana bas.
                        //Form1->Canvas->Pixels[i][j] = Bitmap1->Canvas->Pixels[den1][den2];
                           //bitmap1 = (Bitmap)pictureBox1.Image;

                                      
                        bitmap1.SetPixel(i, j, geciciBitmap.GetPixel((int)den1,(int)den2));
                                                                  
                                           
                        
                    }
                }
            }

            

            var bitmapToShow = ResizeBitmap(bitmap1);

            if (checkBox1.Checked)
            {
                bitmapToShow = SmoothImage(bitmapToShow);
            }

            pictureBox1.Image = bitmapToShow;


        }

        private void resBtn_Click(object sender, EventArgs e)
        {
            resolution = int.Parse(resBox.Text);
            bitmap1 = new Bitmap(resolution, resolution);
        }

        public Bitmap ResizeBitmap(Bitmap inputBitmap)
        {
            Bitmap bitmapToShow = new Bitmap(400, 400);

            // Yeniden boyutlandýrma iþlemi
            using (Graphics g = Graphics.FromImage(bitmapToShow))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(inputBitmap, 0, 0, 400, 400);
            }
            inputBitmap.Dispose();

            return bitmapToShow;
        }

        

        private Bitmap SmoothImage(Bitmap inputImage) // smoothing
        {
            Bitmap smoothedImage = new Bitmap(inputImage.Width, inputImage.Height);

            int windowSize = 3; // Pürüzsüzleþtirme penceresinin boyutu (örnek olarak 3x3)

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    int totalRed = 0;
                    int totalGreen = 0;
                    int totalBlue = 0;

                    int count = 0;

                    for (int offsetY = -windowSize; offsetY <= windowSize; offsetY++)
                    {
                        for (int offsetX = -windowSize; offsetX <= windowSize; offsetX++)
                        {
                            int newX = x + offsetX;
                            int newY = y + offsetY;

                            if (newX >= 0 && newX < inputImage.Width && newY >= 0 && newY < inputImage.Height)
                            {
                                Color pixel = inputImage.GetPixel(newX, newY);
                                totalRed += pixel.R;
                                totalGreen += pixel.G;
                                totalBlue += pixel.B;
                                count++;
                            }
                        }
                    }

                    int avgRed = totalRed / count;
                    int avgGreen = totalGreen / count;
                    int avgBlue = totalBlue / count;

                    Color smoothedColor = Color.FromArgb(avgRed, avgGreen, avgBlue);
                    smoothedImage.SetPixel(x, y, smoothedColor);
                }
            }

            return smoothedImage;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            textBox1.Text = numericUpDown1.Value.ToString();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            textBox2.Text = numericUpDown2.Value.ToString();
        }

 

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            textBox4.Text = numericUpDown4.Value.ToString();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            textBox5.Text = numericUpDown5.Value.ToString();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            textBox6.Text = numericUpDown6.Value.ToString();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            textBox3.Text = numericUpDown3.Value.ToString();
        }



  
    }
}