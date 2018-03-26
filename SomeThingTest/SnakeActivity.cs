using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using SomeThingTest.Model;
using SomeThingTest.Services;
using Xamarin.Android.Things.SenseHAT;

namespace SomeThingTest
{
    [Activity(Label = "Snake", MainLauncher = true)]
    [IntentFilter(new[] { Intent.ActionMain, Intent.CategoryDefault, "android.intent.category.IOT_LAUNCHER" })]
    public class SnakeActivity : Activity
    {
        const int SIZE = 8;
        const int MS_BETWEEN_MOVES = 300;
        readonly ICloudService _cloudService;
        readonly Dictionary<PositionState, Paint> _paints;
        readonly SnakeService _snakeService;
        Action _advanceAction;
        Handler _handler;
        HandlerThread _handlerThread;
        JoystickInputDriver _joystickInputDriver;

        TextView _scoreView;

        ImageView _snakeView;
        bool _started;

        public SnakeActivity()
        {
            _paints = new Dictionary<PositionState, Paint>();
            _snakeService = new SnakeService(SIZE, SIZE);
            _cloudService = new EventHubService();
            _advanceAction = Advance;
        }

        ImageView SnakeView => _snakeView ?? (_snakeView = FindViewById<ImageView>(Resource.Id.SnakeView));
        TextView ScoreView => _scoreView ?? (_scoreView = FindViewById<TextView>(Resource.Id.ScoreView));

        void Advance()
        {
            var advanced = _snakeService.Advance();
            if (!advanced)
            {
                StopGame(true);
                return;
            }

            DrawBoard();
            _handler.PostDelayed(_advanceAction, MS_BETWEEN_MOVES);
        }

        void DrawBoard()
        {
            var board = _snakeService.Board;
            var score = _snakeService.CurrentScore;

            var bitmap = Bitmap.CreateBitmap(SIZE, SIZE, Bitmap.Config.Argb8888);
            using (var canvas = new Canvas(bitmap))
            {
                foreach (var pos in board)
                {
                    canvas.DrawPoint(pos.x, pos.y, GetPaint(pos.state));
                }

                using (var ledMatrix = new LedMatrix())
                {
                    ledMatrix.Draw(bitmap);
                }
            }

            RunOnUiThread(() =>
            {
                SnakeView.SetImageBitmap(Bitmap.CreateScaledBitmap(bitmap, 200, 200, false));
                bitmap.Dispose();
                ScoreView.Text = $"Current score: {_snakeService.CurrentScore}";
            });

            Task.Run(async () => await _cloudService.SendSnakeMove(board, score));
        }

        Paint GetPaint(PositionState state)
        {
            return _paints[state];
        }

        void StartGame()
        {
            if (_started)
            {
                return;
            }

            if (_snakeService.GameEnded)
            {
                _snakeService.Start();
            }

            _started = true;
            _handler.Post(_advanceAction);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _cloudService.Connect();

            SetContentView(Resource.Layout.Snake);

            _handlerThread = new HandlerThread("background");
            _handlerThread.Start();
            _handler = new Handler(_handlerThread.Looper);

            _paints[PositionState.Empty] = new Paint { Color = Color.Argb(50, 255, 255, 255) };
            _paints[PositionState.Snake] = new Paint { Color = Color.Argb(255, 0, 100, 0) };
            _paints[PositionState.Food] = new Paint { Color = Color.Argb(100, 255, 255, 0) };

            _joystickInputDriver = new JoystickInputDriver(new[] { Keycode.Button10, Keycode.Button11, Keycode.Button12, Keycode.Button13, Keycode.Button14 });
        }

        protected override void OnStart()
        {
            base.OnStart();

            _joystickInputDriver.Register();

            _snakeService.Start();
            DrawBoard();
        }

        protected override void OnStop()
        {
            base.OnStop();

            _joystickInputDriver.Unregister();
        }

        protected override void OnPause()
        {
            base.OnPause();
            StopGame();
        }

        void StopGame(bool ended = false)
        {
            _started = false;
            _handler.RemoveCallbacks(_advanceAction);

            if (ended)
            {
                ShowScore(_snakeService.CurrentScore);
            }
        }

        async Task ShowScore(int score)
        {
            var text = $"Your score: {score}";

            RunOnUiThread(() => ScoreView.Text = text);
            foreach (var character in text)
            {
                using (var ledMatrix = new LedMatrix())
                {
                    ledMatrix.Draw(character, Color.Argb(100, 255, 255, 255), Color.Argb(255, 0, 50, 0));
                }

                await Task.Delay(800);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _cloudService.Disconnect();

            _joystickInputDriver?.Dispose();
            _joystickInputDriver = null;

            foreach (var pair in _paints)
            {
                pair.Value.Dispose();
            }

            _paints.Clear();

            _handler.RemoveCallbacks(_advanceAction);
            _handler.Dispose();
            _handler = null;

            _handlerThread.Looper.Quit();
            _handlerThread.Dispose();
            _handlerThread = null;

            _advanceAction = null;

            _snakeView.Dispose();
            _snakeView = null;

            _scoreView.Dispose();
            _scoreView = null;
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            switch (keyCode)
            {
                case Keycode.Button10:
                    _snakeService.Turn(Direction.Left);
                    return true;
                case Keycode.Button11:
                    _snakeService.Turn(Direction.Up);
                    return true;
                case Keycode.Button12:
                    _snakeService.Turn(Direction.Right);
                    return true;
                case Keycode.Button13:
                    _snakeService.Turn(Direction.Down);
                    return true;
                case Keycode.Button14:
                    StartGame();
                    return true;
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}

