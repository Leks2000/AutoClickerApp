using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoClickerApp
{
    public partial class AutoClickerApp : Form
    {
        public AutoClickerApp()
        {
            InitializeComponent();

            RegisterHotKey(this.Handle, HOTKEY_ID, 0, VK_F6);
        }


        [DllImport("user32.dll")] // Убрали лишние CharSet и CallingConvention
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);
        // Импорт функций для работы с глобальными горячими клавишами
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Константы для событий мыши
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        // Идентификатор горячей клавиши (любое уникальное число)
        private const int HOTKEY_ID = 9000;
        // Код клавиши F6
        private const int VK_F6 = 0x75;

        // ----------------------------------------------------
        // 2. Логика Автокликера
        // ----------------------------------------------------

        private bool isClicking = false;
        private Thread clickerThread;
        private int clickInterval = 100; // Значение по умолчанию 100 мс

        // Метод симуляции левого клика
        private void SimulateLeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(10); // Краткая задержка между нажатием и отпусканием
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        // Метод, который будет выполняться в отдельном потоке
        private void ClickerLoop()
        {
            while (isClicking)
            {
                SimulateLeftClick();
                Thread.Sleep(clickInterval);
            }
        }

        private void StartClicker()
        {
            if (isClicking) return;

            // Считывание интервала и проверка корректности
            if (int.TryParse(txtInterval.Text, out int interval) && interval >= 1)
            {
                clickInterval = interval;
            }
            else
            {
                MessageBox.Show("Некорректный интервал. Установите положительное число.");
                return;
            }

            isClicking = true;
            clickerThread = new Thread(ClickerLoop);
            clickerThread.IsBackground = true; // Поток завершится при закрытии приложения
            clickerThread.Start();

            // Обновление UI (нужно использовать Invoke, так как мы в другом потоке)
            this.Invoke((MethodInvoker)delegate {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                this.Text = "AutoClicker (Активен)";
            });
        }

        private void StopClicker()
        {
            isClicking = false;

            // Обновление UI
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            this.Text = "AutoClicker (Остановлен)";
        }

        // ----------------------------------------------------
        // 3. Обработчики событий UI и Горячих клавиш
        // ----------------------------------------------------

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartClicker();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopClicker();
        }

        // Перехват системного сообщения о нажатии горячей клавиши
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && (int)m.WParam == HOTKEY_ID)
            {
                // Нажата F6
                if (isClicking)
                {
                    StopClicker();
                }
                else
                {
                    StartClicker();
                }
            }
            base.WndProc(ref m);
        }

        // Освобождение горячей клавиши при закрытии формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Остановка кликера и отмена регистрации горячей клавиши
            isClicking = false;
            UnregisterHotKey(this.Handle, HOTKEY_ID);
        }
    }
}
