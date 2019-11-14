using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Spine;

namespace ArkcharGenerator
{
    enum FileType
    {
        Image, AlphaImage, Skel, Atlas
    }

    enum AnimType
    {
        Build, Fight
    }

    [Flags]
    enum CheckBit : int
    {
        AlphaImageBuild = 0x00000001,
        ImageBuild = 0x00000010,
        SkelBuild = 0x00000100,
        AtlasBuild = 0x00001000,

        ImageFight = 0x00010000,
        SkelFight = 0x00100000,
        AlphaImageFight = 0x01000000,
        AtlasFight = 0x10000000,
        All = 0x11111111,
        OnlyBuild = 0x00001111
    }
    struct FileInfo
    {
        public string path;
        public FileType fileType;
        public AnimType animType;

        public override string ToString()
        {
            return $"[{animType}|{fileType}]{path}";
        }
    }

    struct SkelInfo
    {
        public string atlasPath;
        public string imagePath;
        public string skelPath;
    }

    struct CharInfo
    {
        public string name;
        public bool hasFightAnima;
    }

    class Program
    {
        private static readonly Dictionary<string, List<FileInfo>> _fileDictionary = new Dictionary<string, List<FileInfo>>();

        static void Main(string[] args)
        {
            Console.WriteLine("输入文件夹");
            var inDir = Console.ReadLine();

            Console.WriteLine("输出文件夹");
            var outDir = Console.ReadLine();

            Console.WriteLine("是否生成战斗动画（不能保证所有战斗动画完好无损）[yes|NO]");
            var generateFightAnim = false;
            var genFightAnima = Console.ReadLine();
            if (genFightAnima?.ToLower() == "yes")
                generateFightAnim = true;

            if (string.IsNullOrEmpty(inDir) || string.IsNullOrEmpty(outDir))
                return;

            Console.WriteLine($"[信息]注册文件");
            foreach (var filePath in Directory.GetFiles(inDir))
                RegisterFile(filePath);

            Console.WriteLine($"[信息]筛选文件");
            CleanRegisteredFile(!generateFightAnim);

            Console.WriteLine($"[信息]生成角色动画");
            Generate(outDir, generateFightAnim);

            Console.WriteLine($"[信息]成功");
        }

        static void Generate(string outDir, bool generateFightAnim)
        {
            var buildCharOut = outDir + "\\BuildChar\\";
            var fightCharOut = outDir + "\\FightChar\\";

            Directory.CreateDirectory(buildCharOut);
            Directory.CreateDirectory(fightCharOut);

            var createdChar = new List<CharInfo>();
            var count = _fileDictionary.Count;
            var current = 0;

            foreach (var fileInfo in _fileDictionary)
            {
                var hasFightAnima = generateFightAnim;

                Console.WriteLine($"[信息]{++current}/{count} '{fileInfo.Key}'");

                // 基建动画
                var buildAtlasPath = GetAtlasPath(fileInfo.Value, AnimType.Build);
                var buildSkelPath = GetSkelPath(fileInfo.Value, AnimType.Build, buildAtlasPath);
                CombineImage(fileInfo.Value, AnimType.Build, buildAtlasPath, buildCharOut + fileInfo.Key + ".png");
                File.Copy(buildSkelPath, buildCharOut + fileInfo.Key + ".skel", true);
                _ = SetAtlasImageAsync(fileInfo.Key, buildAtlasPath, buildCharOut + fileInfo.Key + ".atlas");

                // 战斗动画（beta）
                if (generateFightAnim)
                {
                    var fightAtlasPath = GetAtlasPath(fileInfo.Value, AnimType.Fight);
                    if (buildAtlasPath == "" || fightAtlasPath == "")
                    {
                        Console.WriteLine("[警告]暂不支持自动加载的角色： " + fileInfo.Key + " 因为程序无法自动找出正面动画");
                        hasFightAnima = false;
                    }
                    else
                    {
                        var fightSkelPath = GetSkelPath(fileInfo.Value, AnimType.Fight, fightAtlasPath);
                        CombineImage(fileInfo.Value, AnimType.Fight, fightAtlasPath,
                            fightCharOut + fileInfo.Key + ".png");
                        File.Copy(fightSkelPath, fightCharOut + fileInfo.Key + ".skel", true);
                        _ = SetAtlasImageAsync(fileInfo.Key, fightAtlasPath, fightCharOut + fileInfo.Key + ".atlas");
                    }
                }

                // 添加角色
                createdChar.Add(new CharInfo{name = fileInfo.Key, hasFightAnima = hasFightAnima });
            }

            Console.WriteLine($"[信息]开始创建角色列表");

            using (var fileWriter = new StreamWriter(File.Create(outDir + "\\CharInfo.txt")))
            {
                foreach (var charInfo in createdChar)
                    fileWriter.WriteLine($"{charInfo.name}={charInfo.name}={charInfo.hasFightAnima}");
            }
        }

        static async Task SetAtlasImageAsync(string characterName, string atlasPath, string outputPath)
        {
            using (var fileReadStream = File.OpenRead(atlasPath))
            using (var fileReader = new StreamReader(fileReadStream))
            using (var fileWriteStream = File.Create(outputPath))
            {
                var result = $"\n{characterName}.png\n";
                fileReader.ReadLine();
                fileReader.ReadLine();

                result += fileReader.ReadToEnd();
                var byteArray = Encoding.UTF8.GetBytes(result);
                await fileWriteStream.WriteAsync(byteArray, 0, byteArray.Length);
            }
        }

        static Bitmap GetAlphaImageBitmap(List<FileInfo> fileInfoList, AnimType animType, Atlas atlas)
        {
            foreach (var fileInfo in fileInfoList)
            {
                if (fileInfo.animType != animType || fileInfo.fileType != FileType.AlphaImage)
                    continue;

                if (animType == AnimType.Build)
                    return (Bitmap) Image.FromFile(fileInfo.path);

                using (var bitmap = Image.FromFile(fileInfo.path))
                {
                    if (bitmap.Width == atlas.GetPage()[0].width && bitmap.Height == atlas.GetPage()[0].height)
                        return (Bitmap) Image.FromFile(fileInfo.path);
                }
            }

            return null;
        }

        static void CombineImage(Bitmap bitmap, Bitmap alphaBitmap, string outputPath)
        {
            for (var x = 0; x < bitmap.Width; ++x)
            {
                for (var y = 0; y < bitmap.Height; ++y)
                {
                    var currentColor = bitmap.GetPixel(x, y);
                    var alpha = alphaBitmap.GetPixel(x, y).B;

                    bitmap.SetPixel(x, y, Color.FromArgb(alpha, currentColor.R, currentColor.G, currentColor.B));
                }
            }

            bitmap.Save(outputPath);
        }

        static void CombineImage(List<FileInfo> fileInfoList, AnimType animType, string atlasPath, string outputPath)
        {
            using (var atlas = new Atlas(atlasPath, new GeneratorTextureLoader()))
            {
                foreach (var fileInfo in fileInfoList)
                {
                    if (fileInfo.animType != animType || fileInfo.fileType != FileType.Image)
                        continue;

                    using (var bitmap = (Bitmap) Image.FromFile(fileInfo.path))
                    {
                        if (bitmap.Width != atlas.GetPage()[0].width || bitmap.Height != atlas.GetPage()[0].height)
                            continue;

                        var alphaBitmap = GetAlphaImageBitmap(fileInfoList, animType, atlas);

                        CombineImage(bitmap, alphaBitmap, outputPath);
                    }
                }
            }
        }

        static string GetSkelPath(List<FileInfo> fileInfoList, AnimType animType, string atlasPath)
        {
            using (var atlas = new Atlas(atlasPath, new GeneratorTextureLoader()))
            {
                foreach (var fileInfo in fileInfoList)
                {
                    if (fileInfo.animType != animType || fileInfo.fileType != FileType.Skel)
                        continue;

                    if (animType == AnimType.Build)
                        return fileInfo.path;

                    try
                    {
                        var binary = new SkeletonBinary(atlas);
                        binary.ReadSkeletonData(fileInfo.path);
                        return fileInfo.path;
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }

                return "";
            }
        }

        static string GetAtlasPath(List<FileInfo> fileInfoList, AnimType animType)
        {
            foreach (var fileInfo in fileInfoList)
            {
                if (fileInfo.animType != animType || fileInfo.fileType != FileType.Atlas)
                    continue;

                if (animType == AnimType.Build)
                    return fileInfo.path;

                using (var atlas = new Atlas(fileInfo.path, new GeneratorTextureLoader()))
                {
                    var page = atlas.GetPage()[0];
                    if (page.width == 512 && page.height == 512)
                        return fileInfo.path;
                }
            }

            return "";
        }

        static void CleanRegisteredFile(bool onlyBuild)
        {
            var discardList = new List<string>();

            foreach (var pair in _fileDictionary)
            {
                CheckBit checkResult = 0;

                var fileList = pair.Value;

                foreach (var fileInfo in fileList)
                {
                    var isFight = fileInfo.animType == AnimType.Fight;

                    switch (fileInfo.fileType)
                    {
                        case FileType.AlphaImage:
                            if (isFight) 
                                checkResult |= CheckBit.AlphaImageFight;
                            else
                                checkResult |= CheckBit.AlphaImageBuild;
                            break;
                        case FileType.Image:
                            if (isFight)
                                checkResult |= CheckBit.ImageFight;
                            else
                                checkResult |= CheckBit.ImageBuild;
                            break;
                        case FileType.Skel:
                            if (isFight)
                                checkResult |= CheckBit.SkelFight;
                            else
                                checkResult |= CheckBit.SkelBuild;
                            break;
                        case FileType.Atlas:
                            if (isFight)
                                checkResult |= CheckBit.AtlasFight;
                            else
                                checkResult |= CheckBit.AtlasBuild;
                            break;
                    }
                }

                if (onlyBuild)
                {
                    if (checkResult != CheckBit.All)
                        discardList.Add(pair.Key);
                }
                else
                {
                    if (checkResult < CheckBit.OnlyBuild)
                        discardList.Add(pair.Key);
                }
            }

            foreach (var discardItem in discardList)
                _fileDictionary.Remove(discardItem);
        }

        static void RegisterFile(string path)
        {
            var fileName = new System.IO.FileInfo(path).Name;
            string animName;
            bool hasAlphaKey;
            AnimType animType;

            if (fileName.StartsWith("build_char_"))
            {
                animType = AnimType.Build;
                var nameWithExtension = fileName.Substring(15);
                animName = nameWithExtension.Substring(0, nameWithExtension.IndexOf('.'));
            }
            else if (fileName.StartsWith("char_"))
            {
                animType = AnimType.Fight;
                var nameWithExtension = fileName.Substring(9);
                animName = nameWithExtension.Substring(0, nameWithExtension.IndexOf('.'));
            }
            else
                return;

            hasAlphaKey = animName.Contains("[alpha]");
            animName = animName.Replace("[alpha]", "");
            if (animName.IndexOf('#') >= 0)
                animName = animName.Substring(0, animName.IndexOf('#'));
            animName = animName.Replace(" ", "");

            if (!_fileDictionary.ContainsKey(animName))
                _fileDictionary[animName] = new List<FileInfo>();

            var fileExtensions = path.Split('.');


            if (fileExtensions[fileExtensions.Length - 1] == "png")
                _fileDictionary[animName].Add(new FileInfo {path = path, fileType = hasAlphaKey ? FileType.AlphaImage : FileType.Image, animType = animType});
            else if (fileExtensions[fileExtensions.Length - 1] == "txt")
            {
                if (fileExtensions[fileExtensions.Length - 2].StartsWith("skel"))
                    _fileDictionary[animName].Add(new FileInfo { path = path, fileType = FileType.Skel, animType = animType });
                else if (fileExtensions[fileExtensions.Length - 2].StartsWith("atlas"))
                    _fileDictionary[animName].Add(new FileInfo { path = path, fileType = FileType.Atlas, animType = animType });
            }
        }
    }
}
