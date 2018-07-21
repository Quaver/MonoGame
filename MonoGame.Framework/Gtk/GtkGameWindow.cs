// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Gtk;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Viewport = Microsoft.Xna.Framework.Graphics.Viewport;

namespace Microsoft.Xna.Framework
{
    class GtkGameWindow : GameWindow
    {
        public static GLArea TempGLArea;

        private EventBox _eventArea;
        private Game _game;
        private GLArea _glarea;
        private int _isExiting;
        private List<Keys> _keys;

        public GtkGameWindow(Game game)
        {
            _keys = new List<Keys>();

            _game = game;

            _eventArea = new EventBox();
            _eventArea.AddEvents((int)Gdk.EventMask.PointerMotionMask);
            _eventArea.ButtonPressEvent += EventArea_ButtonPressEvent;
            _eventArea.ButtonReleaseEvent += EventArea_ButtonReleaseEvent;
            _eventArea.MotionNotifyEvent += EventBox_MotionNotifyEvent;

            TempGLArea = _glarea = new GLArea();
            _glarea.AutoRender = true;
            _glarea.HasDepthBuffer = true;
            _glarea.HasStencilBuffer = false;
            _glarea.SizeAllocated += GLArea_SizeAllocated;

            _eventArea.Child = _glarea;

            game.Services.AddService<Widget>(_eventArea);
            game.Services.AddService<GLArea>(_glarea);
        }

        public override bool AllowUserResizing
        {
            get => false; set { }
        }

        public override Rectangle ClientBounds => new Rectangle(0, 0, _glarea.AllocatedWidth, _glarea.AllocatedHeight);

        public override Point Position
        {
            get => Point.Zero; set { }
        }

        public override DisplayOrientation CurrentOrientation => DisplayOrientation.Default;

        public override IntPtr Handle => _glarea.Handle;

        public override string ScreenDeviceName => string.Empty;

        private void EventArea_ButtonPressEvent(object sender, ButtonPressEventArgs args)
        {
            switch (args.Event.Button)
            {
                case 1:
                    MouseState.LeftButton = ButtonState.Pressed;
                    break;
                case 2:
                    MouseState.MiddleButton = ButtonState.Pressed;
                    break;
                case 3:
                    MouseState.RightButton = ButtonState.Pressed;
                    break;
                case 4:
                    MouseState.XButton1 = ButtonState.Pressed;
                    break;
                case 5:
                    MouseState.XButton2 = ButtonState.Pressed;
                    break;
            }
        }

        private void EventArea_ButtonReleaseEvent(object sender, ButtonReleaseEventArgs args)
        {
            switch (args.Event.Button)
            {
                case 1:
                    MouseState.LeftButton = ButtonState.Released;
                    break;
                case 2:
                    MouseState.MiddleButton = ButtonState.Released;
                    break;
                case 3:
                    MouseState.RightButton = ButtonState.Released;
                    break;
                case 4:
                    MouseState.XButton1 = ButtonState.Released;
                    break;
                case 5:
                    MouseState.XButton2 = ButtonState.Released;
                    break;
            }
        }

        [GLib.ConnectBefore]
        private void EventBox_KeyPressEvent(object sender, KeyPressEventArgs args)
        {
            var xnakey = KeyboardUtil.ToXna(args.Event.HardwareKeycode);
            if (!_keys.Contains(xnakey))
            {
                _keys.Add(xnakey);
                Keyboard.SetKeys(_keys);
            }
        }

        [GLib.ConnectBefore]
        private void EventBox_KeyReleaseEvent(object sender, KeyReleaseEventArgs args)
        {
            var xnakey = KeyboardUtil.ToXna(args.Event.HardwareKeycode);
            _keys.Remove(xnakey);
            Keyboard.SetKeys(_keys);
        }

        private void EventBox_MotionNotifyEvent(object sender, MotionNotifyEventArgs args)
        {
            int outx, outy;
            _eventArea.Toplevel.TranslateCoordinates(_eventArea, (int)args.Event.X, (int)args.Event.Y, out outx, out outy);

            MouseState.X = outx;
            MouseState.Y = outy;
        }

        private void GLArea_SizeAllocated(object sender, EventArgs args)
        {
            if (_game.GraphicsDevice == null || (
                _game.GraphicsDevice.PresentationParameters.BackBufferWidth == _glarea.AllocatedWidth &&
                _game.GraphicsDevice.PresentationParameters.BackBufferHeight == _glarea.AllocatedHeight
            ))
                return;

            _game.GraphicsDevice.PresentationParameters.BackBufferWidth = _glarea.AllocatedWidth;
            _game.GraphicsDevice.PresentationParameters.BackBufferHeight = _glarea.AllocatedHeight;
            _game.GraphicsDevice.Viewport = new Viewport(0, 0, _glarea.AllocatedWidth, _glarea.AllocatedHeight);

            OnClientSizeChanged();
        }

        private void GLArea_Rendered(object sender, EventArgs args)
        {
            if (_isExiting > 0)
                return;

            _game.RunOneFrame();
            _glarea.QueueRender();
        }

        public void StartRunLoop()
        {
            _eventArea.Toplevel.FocusOutEvent += (o, e) => _keys.Clear();
            _eventArea.Toplevel.AddEvents((int)Gdk.EventMask.KeyPressMask);
            _eventArea.Toplevel.KeyPressEvent += EventBox_KeyPressEvent;
            _eventArea.Toplevel.AddEvents((int)Gdk.EventMask.KeyReleaseMask);
            _eventArea.Toplevel.KeyReleaseEvent += EventBox_KeyReleaseEvent;

            GLArea_SizeAllocated(null, EventArgs.Empty);

            _glarea.Render += GLArea_Rendered;
            _glarea.QueueRender();
        }

        public void Exit()
        {
            _isExiting++;
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {

        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {

        }

        protected override void SetTitle(string title)
        {

        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {

        }
    }
}
