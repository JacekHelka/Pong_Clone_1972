using System;
using System.Drawing;
using System.Windows.Forms;

namespace PongGame
{
    public partial class Form1 : Form
    {
        // Zmienna prędkość piłki
        float ballSpeed = 12f;
        const float ballSpeedIncrement = 2f;
        const float ballSpeedMax = 35f;
        float ballDirX = 3f, ballDirY = 2f; 
        float ballPosX, ballPosY;
        const int ballSize = 20;
        int paddleSpeed = 30;
        Rectangle ball;
        Rectangle paddleLeft, paddleRight;
        int scoreLeft = 0, scoreRight = 0;
        Timer timer = new Timer();

        private bool moveUpLeft = false, moveDownLeft = false;
        private bool moveUpRight = false, moveDownRight = false;

        bool gameOver = false;
        int winner = 0; // 1 = lewy, 2 = prawy
        const int maxScore = 11;

        bool showStartScreen = true;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Text = "PONG";
            Cursor.Hide();

            timer.Interval = 16;
            timer.Tick += Timer_Tick;

            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.FormClosed += Form1_FormClosed;
            this.Load += new System.EventHandler(this.Form1_Load);
        }

        private void ResetGame()
        {
            int paddleWidth = 20;
            int paddleHeight = 100;
            int paddleMargin = 30;
            ballPosX = this.ClientSize.Width / 2f - ballSize / 2f;
            ballPosY = this.ClientSize.Height / 2f - ballSize / 2f;
            ball = new Rectangle((int)ballPosX, (int)ballPosY, ballSize, ballSize);
            paddleLeft = new Rectangle(paddleMargin, this.ClientSize.Height / 2 - paddleHeight / 2, paddleWidth, paddleHeight);
            paddleRight = new Rectangle(this.ClientSize.Width - paddleMargin - paddleWidth, this.ClientSize.Height / 2 - paddleHeight / 2, paddleWidth, paddleHeight);

            // Losowy kierunek startowy
            Random rnd = new Random();
            ballDirX = rnd.Next(0, 2) == 0 ? 1f : -1f;
            ballDirY = (float)(rnd.NextDouble() * 2 - 1); // -1 do 1
            NormalizeBallDirection();
            ballSpeed = 12f;
        }

        private void FullReset()
        {
            scoreLeft = 0;
            scoreRight = 0;
            gameOver = false;
            winner = 0;
            ResetGame();
            timer.Start();
        }

        private void NormalizeBallDirection()
        {
            float length = (float)Math.Sqrt(ballDirX * ballDirX + ballDirY * ballDirY);
            if (length != 0)
            {
                ballDirX /= length;
                ballDirY /= length;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Ruch paletek
            if (moveUpLeft && paddleLeft.Top > 0)
                paddleLeft.Y -= paddleSpeed;
            if (moveDownLeft && paddleLeft.Bottom < this.ClientSize.Height)
                paddleLeft.Y += paddleSpeed;
            if (moveUpRight && paddleRight.Top > 0)
                paddleRight.Y -= paddleSpeed;
            if (moveDownRight && paddleRight.Bottom < this.ClientSize.Height)
                paddleRight.Y += paddleSpeed;

            // Ruch piłki z aktualną prędkością
            ballPosX += ballDirX * ballSpeed;
            ballPosY += ballDirY * ballSpeed;
            ball = new Rectangle((int)ballPosX, (int)ballPosY, ballSize, ballSize);

            // Odbicie od góry/dół
            if (ball.Top <= 0 || ball.Bottom >= this.ClientSize.Height)
            {
                ballDirY = -ballDirY;
                NormalizeBallDirection();
            }

            // Odbicie od paletek
            if (ball.IntersectsWith(paddleLeft))
            {
                ballDirX = Math.Abs(ballDirX); // zawsze w prawo
                ballDirY = GetBounceAngle(ball, paddleLeft);
                NormalizeBallDirection();
                ballSpeed = Math.Min(ballSpeed + ballSpeedIncrement, ballSpeedMax);
            }
            if (ball.IntersectsWith(paddleRight))
            {
                ballDirX = -Math.Abs(ballDirX); // zawsze w lewo
                ballDirY = GetBounceAngle(ball, paddleRight);
                NormalizeBallDirection();
                ballSpeed = Math.Min(ballSpeed + ballSpeedIncrement, ballSpeedMax);
            }

            // Punktacja
            if (!gameOver)
            {
                if (ball.Left <= 0)
                {
                    scoreRight++;
                    if (scoreRight >= maxScore)
                    {
                        gameOver = true;
                        winner = 2;
                        showStartScreen = true;
                        timer.Stop();
                    }
                    else
                    {
                        ResetGame();
                    }
                }
                else if (ball.Right >= this.ClientSize.Width)
                {
                    scoreLeft++;
                    if (scoreLeft >= maxScore)
                    {
                        gameOver = true;
                        winner = 1;
                        showStartScreen = true;
                        timer.Stop();
                    }
                    else
                    {
                        ResetGame();
                    }
                }
            }

            this.Invalidate();
        }

        // Kąt odbicia zależny od miejsca trafienia w paletkę
        private float GetBounceAngle(Rectangle ball, Rectangle paddle)
        {
            float relativeIntersectY = (paddle.Top + paddle.Height / 2f) - (ball.Top + ball.Height / 2f);
            float normalizedRelativeIntersectionY = relativeIntersectY / (paddle.Height / 2f);
            float bounceAngle = normalizedRelativeIntersectionY * 1.2f; // max ok. 1.2 radiana
            return -bounceAngle;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Black);

            // Linia środkowa
            using (Pen pen = new Pen(Color.White, 4))
            {
                for (int y = 0; y < this.ClientSize.Height; y += 40)
                    g.DrawLine(pen, this.ClientSize.Width / 2, y, this.ClientSize.Width / 2, y + 20);
            }

            g.FillRectangle(Brushes.White, paddleLeft);
            g.FillRectangle(Brushes.White, paddleRight);
            g.FillEllipse(Brushes.White, ball);

            // Parametry segmentowych cyfr
            float digitSize = this.ClientSize.Height * 0.10f;
            float digitSpacing = digitSize * 0.2f;
            float scoreY = this.ClientSize.Height * 0.05f;

            // Lewy wynik
            string leftScoreStr = scoreLeft.ToString();
            float leftHalfCenter = this.ClientSize.Width / 4f;
            float leftTotalWidth = leftScoreStr.Length * digitSize + (leftScoreStr.Length - 1) * digitSpacing;
            float leftStartX = leftHalfCenter - leftTotalWidth / 2f;
            for (int i = 0; i < leftScoreStr.Length; i++)
            {
                int digit = leftScoreStr[i] - '0';
                float x = leftStartX + i * (digitSize + digitSpacing);
                DrawPongDigit(g, digit, x, scoreY, digitSize, Brushes.White);
            }

            // Prawy wynik
            string rightScoreStr = scoreRight.ToString();
            float rightHalfCenter = this.ClientSize.Width * 3f / 4f;
            float rightTotalWidth = rightScoreStr.Length * digitSize + (rightScoreStr.Length - 1) * digitSpacing;
            float rightStartX = rightHalfCenter - rightTotalWidth / 2f;
            for (int i = 0; i < rightScoreStr.Length; i++)
            {
                int digit = rightScoreStr[i] - '0';
                float x = rightStartX + i * (digitSize + digitSpacing);
                DrawPongDigit(g, digit, x, scoreY, digitSize, Brushes.White);
            }

            // Napis WIN po stronie zwycięzcy
            if (gameOver)
            {
                using (Font winFont = new Font("Courier New", 120, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    string winText = "WIN";
                    SizeF winSize = g.MeasureString(winText, winFont);
                    float winY = this.ClientSize.Height / 3f  - winSize.Height / 2f;
                    float winX;
                    if (winner == 1)
                        winX = this.ClientSize.Width / 4f - winSize.Width / 2f; // lewa połowa
                    else
                        winX = this.ClientSize.Width * 3f / 4f - winSize.Width / 2f; // prawa połowa

                    g.DrawString(winText, winFont, Brushes.White, winX, winY);
                }
            }

            // Overlay startowy (nakładka)
            if (showStartScreen)
            {
                int overlayWidth = (int)(this.ClientSize.Width * 0.6);
                int overlayHeight = (int)(this.ClientSize.Height * 0.35);
                int overlayX = (this.ClientSize.Width - overlayWidth) / 2;
                int overlayY = (this.ClientSize.Height - overlayHeight) / 4 * 3;

                using (SolidBrush overlayBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0))) // półprzezroczysty czarny
                {
                    g.FillRectangle(overlayBrush, overlayX, overlayY, overlayWidth, overlayHeight);
                }
                using (Pen borderPen = new Pen(Color.White, 3))
                {
                    g.DrawRectangle(borderPen, overlayX, overlayY, overlayWidth, overlayHeight);
                }

                string info1 = "LEFT: W/S";
                string info2 = "RIGHT: UP/DOWN";
                string info3 = "Press SPACE to start";
                using (Font font = new Font("Courier New", 48, FontStyle.Bold, GraphicsUnit.Pixel))
                using (Font fontSmall = new Font("Courier New", 32, FontStyle.Bold, GraphicsUnit.Pixel))
                {
                    SizeF size1 = g.MeasureString(info1, fontSmall);
                    SizeF size2 = g.MeasureString(info2, fontSmall);
                    SizeF size3 = g.MeasureString(info3, font);

                    float y = overlayY + overlayHeight / 2f - (size1.Height + size2.Height + size3.Height + 40) / 2f;
                    g.DrawString(info1, fontSmall, Brushes.White, this.ClientSize.Width / 2f - size1.Width / 2f, y);
                    g.DrawString(info2, fontSmall, Brushes.White, this.ClientSize.Width / 2f - size2.Width / 2f, y + size1.Height + 10);
                    g.DrawString(info3, font, Brushes.White, this.ClientSize.Width / 2f - size3.Width / 2f, y + size1.Height + size2.Height + 40);
                }
            }
        }

        // Rysowanie cyfry w stylu Pong (segmenty)
        private void DrawPongDigit(Graphics g, int digit, float x, float y, float size, Brush brush)
        {
            // Segmenty: 0 = góra, 1 = lewy górny, 2 = prawy górny, 3 = środek, 4 = lewy dolny, 5 = prawy dolny, 6 = dół
            bool[][] segments = new bool[][]
            {
                new[] { true,  true,  true, false, true,  true,  true  }, // 0
                new[] { false, false, true, false, false, true,  false }, // 1
                new[] { true,  false, true,  true,  true,  false, true  }, // 2
                new[] { true,  false, true,  true,  false, true,  true  }, // 3
                new[] { false, true,  true,  true,  false, true,  false }, // 4
                new[] { true,  true,  false, true,  false, true,  true  }, // 5
                new[] { true,  true,  false, true,  true,  true,  true  }, // 6
                new[] { true,  false, true,  false, false, true,  false }, // 7
                new[] { true,  true,  true,  true,  true,  true,  true  }, // 8
                new[] { true,  true,  true,  true,  false, true,  true  }, // 9
            };

            float w = size;
            float h = size * 2;
            float thickness = size * 0.3f; // segmenty zajmują połowę szerokości/połowę wysokości
            var seg = segments[digit];

            // Góra
            if (seg[0]) g.FillRectangle(brush, x, y, w, thickness);
            // Lewy górny
            if (seg[1]) g.FillRectangle(brush, x, y, thickness, h / 2);
            // Prawy górny
            if (seg[2]) g.FillRectangle(brush, x + w - thickness, y, thickness, h / 2);
            // Środek
            if (seg[3]) g.FillRectangle(brush, x, y + h / 2 - thickness / 2, w, thickness);
            // Lewy dolny
            if (seg[4]) g.FillRectangle(brush, x, y + h / 2, thickness, h / 2);
            // Prawy dolny
            if (seg[5]) g.FillRectangle(brush, x + w - thickness, y + h / 2, thickness, h / 2);
            // Dół
            if (seg[6]) g.FillRectangle(brush, x, y + h - thickness, w, thickness);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (showStartScreen)
            {
                if (e.KeyCode == Keys.Space)
                {
                    showStartScreen = false;
                    gameOver = false;
                    winner = 0;
                    scoreLeft = 0;
                    scoreRight = 0;
                    ResetGame();
                    timer.Start();
                    Invalidate();
                }
                if (e.KeyCode == Keys.Escape) this.Close();
                return;
            }
            if (e.KeyCode == Keys.W) moveUpLeft = true;
            if (e.KeyCode == Keys.S) moveDownLeft = true;
            if (e.KeyCode == Keys.Up) moveUpRight = true;
            if (e.KeyCode == Keys.Down) moveDownRight = true;
            if (e.KeyCode == Keys.Escape) this.Close();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) moveUpLeft = false;
            if (e.KeyCode == Keys.S) moveDownLeft = false;
            if (e.KeyCode == Keys.Up) moveUpRight = false;
            if (e.KeyCode == Keys.Down) moveDownRight = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cursor.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            showStartScreen = true;
            timer.Stop();
            Invalidate();
        }
    }
}
