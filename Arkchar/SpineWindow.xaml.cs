using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Arkchar.Renderer;
using SDL2;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.Version;
using SharpGL.WPF;
using Spine;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Arkchar
{
    /// <summary>
    /// SpineWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SpineWindow : Window
    {
        private uint _lastTimeUpdate;

        public ICharUpdater updater;

        public bool isGLReady = false;

        public Skeleton skeleton;
        public AnimationStateData animationData;
        public AnimationState animation;
        public SkeletonMeshRenderer skeletonRenderer;

        public string currentAnimPath;

        public bool isBuildChar;

        public void SwitchAnimation(bool buildChar, string path)
        {
            if (!isGLReady)
                return;

            string skeletonName;
            if (buildChar)
                skeletonName = "BuildChar." + path;
            else
                skeletonName = "FightChar." + path;

            skeleton = SkeletonLoader.Load(skeletonName);
            animationData = new AnimationStateData(skeleton.Data);
            animation = new AnimationState(animationData);

            animationData.DefaultMix = 0.5f;

            isBuildChar = buildChar;

            if (isBuildChar)
                updater = new BuildCharUpdater();
            else
                updater = new FightCharUpdater();

            currentAnimPath = path;
            skeleton?.UpdateCache();

            updater?.Init(this);
        }

        public SpineWindow()
        {
            InitializeComponent();
            Native.LibraryLoader.Init();
            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);

            LoadCharInfo();
        }

        public void LoadCharInfo()
        {
            var file = File.OpenRead(Environment.CurrentDirectory + "\\Assets\\CharInfo.txt");
            var fileReader = new StreamReader(file);

            while (!fileReader.EndOfStream)
            {
                var line = fileReader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                var pair = line.Split('=');

                if (pair.Length != 2)
                    continue;

                var name = pair[0];
                var filePath = pair[1];

                CharNameComboBox.Items.Add(new CharData(name, filePath));
            }
        }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //  Get the OpenGL instance that's been passed to us.
            var gl = args.OpenGL;

            //  Clear the color and depth buffers.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            var currentTime = SDL.SDL_GetTicks();
            var deltaTime = currentTime - _lastTimeUpdate;

            animation?.Update(deltaTime / 1000.0f);
            animation?.Apply(skeleton);
            skeleton?.UpdateWorldTransform();
            
            skeletonRenderer.Draw(gl, skeleton);

            _lastTimeUpdate = currentTime;

            // Refresh
            updater?.Update(this);

            // Flush OpenGL
            gl.Flush();

            // Auto hidden
            if (!_isMouseInWindow && SDL.SDL_GetTicks() - _lastTimeMouseLeave > 10000)
                ControlPanel.Visibility = Visibility.Hidden;
        }

        private void OpenGLControl_OnOpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            var gl = args.OpenGL;

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_BLEND);

            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            SkeletonLoader.gl = gl;

            skeletonRenderer = new SkeletonMeshRenderer(gl);

            isGLReady = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (Math.Abs(ActualWidth) < float.Epsilon || 
                Math.Abs(ActualHeight) < float.Epsilon)
                return;

            var centerPos = PointToScreen(new Point(ActualWidth / 2, ActualHeight / 2));

            skeleton.FlipX = centerPos.X > Screen.PrimaryScreen.WorkingArea.Width / 2.0f;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            updater?.OnMousePress(this);
        }

        private void SwitchAnimation()
        {
            var charData = (CharData) CharNameComboBox?.SelectedItem;
            if (charData == null || BuildCharRadioButton.IsChecked == null)
                return;

            var path = charData.FilePath;
            var buildChar = BuildCharRadioButton.IsChecked.Value;

            try
            {
                SwitchAnimation(buildChar, path);
            }
            catch (Exception e)
            {
                MessageBox.Show($"未能成功加载 {charData.Name} 的骨骼动画，发生错误: {e.Message}");
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SwitchAnimation();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CharNameComboBox.SelectedItem != null)
                SwitchAnimation();
            else
                CharNameComboBox.SelectedValue = currentAnimPath;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            currentAnimPath = "spot";
            CharNameComboBox.SelectedValue = currentAnimPath;
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseInWindow = true;
            ControlPanel.Visibility = Visibility.Visible;
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            ControlPanel.Visibility = Visibility.Hidden;
        }

        private void ExitButton_OnClickButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private uint _lastTimeMouseLeave;
        private bool _isMouseInWindow;

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseInWindow = false;
            _lastTimeMouseLeave = SDL.SDL_GetTicks();
        }
    }
}
