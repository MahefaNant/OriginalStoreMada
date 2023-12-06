using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace originalstoremada.C_;

public class ImageService
{
    private const int MaxImageSize = 15000000; // Max file size: 15MB

    public static List<string> UploadAndResizeImages(IWebHostEnvironment _webHostEnvironment,Guid guid, List<IFormFile> imageFiles, string path,int DesiredWidth, int DesiredHeight, bool minimize)
    {
        List<string> savedFileNames = new List<string>();

        try
        {
            VerifFormatImage(imageFiles);
        }
        catch (Exception e)
        {
            throw new ArgumentException("format Image(s) Invalide(s)");
        }

        foreach (var imageFile in imageFiles)
        {
            if (imageFile.Length > 0)
            {
                if (imageFile.Length > MaxImageSize)
                {
                    throw new ArgumentException("La taille de l'image ne doit pas dépasser 15 Mo.");
                }

                using var image = Image.Load<Rgba32>(imageFile.OpenReadStream());

                // Resize the image to the desired size
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(DesiredWidth, DesiredHeight),
                    Mode = ResizeMode.Max
                }));

                int offsetX = (DesiredWidth - image.Width) / 2;
                int offsetY = (DesiredHeight - image.Height) / 2;
                

                string uniqueFileName = minimize ? "minimize_" + guid + "_" +  imageFile.FileName : guid + "_" +  imageFile.FileName;
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, path, uniqueFileName);
                
                // Extract dominant color from the resized image
                Rgba32 backgroundColor = ExtractDominantColor(image);

                // Create a new image with the desired size and background color
                using (var centeredImage = new Image<Rgba32>(DesiredWidth, DesiredHeight))
                {
                    centeredImage.Mutate(x => x
                        .BackgroundColor(new Rgba32(backgroundColor.R, backgroundColor.G, backgroundColor.B, 255))
                        .DrawImage(image, new Point(offsetX, offsetY), 1.0f));

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        centeredImage.Save(stream, new JpegEncoder());
                    }
                }

                savedFileNames.Add(uniqueFileName);
            }
        }
        
        return savedFileNames;
    }
    

    public static void RemoveImages(IWebHostEnvironment _webHostEnvironment,string path, List<string> Image, bool removeMini)
    {

        if (Image.Any())
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            string repertoireComplet = Path.Combine(webRootPath, path);
            foreach (var I in Image)
            {
                string cheminImage = Path.Combine(repertoireComplet, I);
                string cheminImage2 = removeMini? Path.Combine(repertoireComplet, $"minimize_{I}") : "";
                
                try
                {
                    if (File.Exists(cheminImage))
                    {
                        File.Delete(cheminImage);
                    }

                    if (removeMini && File.Exists(cheminImage2))
                    {
                        File.Delete(cheminImage2);
                    }
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
        }
    }
    
    public static void CopyFile(IWebHostEnvironment _webHostEnvironment, string sourcePath, string destinationPath)
    {
        string webRootPath = _webHostEnvironment.WebRootPath;
        string sourceFilePath = Path.Combine(webRootPath, sourcePath);
        string destinationFilePath = Path.Combine(webRootPath, destinationPath);

        try
        {
            File.Copy(sourceFilePath, destinationFilePath, true);
            // Le troisième paramètre "true" permet de remplacer le fichier de destination s'il existe déjà.
            Console.WriteLine("Le fichier a été copié avec succès.");
        }
        catch (IOException ex)
        {
            // Gérer l'exception d'entrée/sortie (par exemple, le fichier source n'existe pas).
            Console.WriteLine("Erreur d'entrée/sortie : " + ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Gérer l'exception d'accès non autorisé (par exemple, pas de droits pour écrire dans le répertoire de destination).
            Console.WriteLine("Accès non autorisé : " + ex.Message);
        }
        catch (Exception ex)
        {
            // Gérer les autres exceptions génériques.
            Console.WriteLine("Erreur inattendue : " + ex.Message);
        }
    }
    
    private  static Rgba32 ExtractDominantColor(Image<Rgba32> image)
    {
        var pixelMemory = image.GetPixelMemoryGroup();

        var colorFrequencies = new Dictionary<Rgba32, int>();

        foreach (var pixelRow in pixelMemory)
        {
            foreach (var pixel in pixelRow.Span)
            {
                var rgbaColor = pixel;

                if (!colorFrequencies.ContainsKey(rgbaColor))
                {
                    colorFrequencies[rgbaColor] = 0;
                }
                colorFrequencies[rgbaColor]++;
            }
        }

        var mostFrequentColor = colorFrequencies.OrderByDescending(pair => pair.Value).FirstOrDefault();

        return mostFrequentColor.Key;
    }
    
    private static string UniqueFileName(string originalFileName)
    {
        string uniqueFileName = Guid.NewGuid()+ "_" + originalFileName;
        return uniqueFileName;
    }

    public static void VerifFormatImage(List<IFormFile> imageFiles)
    {
        foreach (var im in imageFiles)
        {
            using var image = Image.Load(im.OpenReadStream());
        }
    }
}