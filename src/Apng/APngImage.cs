using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using gInk.Extensions;
using System.Threading.Tasks;
using gInk.Apng.Chunks;
using System.Drawing;
using System.Drawing.Imaging;

/// <summary>
///     freely copied from  https://github.com/ImoutoChan/ApngWpfPlayer/tree/master/ImoutoRebirth.Navigator.ApngWpfPlayer/ApngEngine
///     code has been refactored and slightly adjusted
/// </summary>
namespace gInk.Apng
{
    public class ApngImage
    {
        private readonly List<Frame> _frames = new List<Frame>();

        public ApngImage(string fileName)
            : this(File.ReadAllBytes(fileName))
        {
        }

        private ApngImage(byte[] fileBytes)
        {
            // first we try to identify the type of stream            
            MemoryStream ms = new MemoryStream(fileBytes);
            Bitmap img = new Bitmap(ms);

            ms.Seek(0, SeekOrigin.Begin);

            // if GIF (animated or not )
            if (ImageFormat.Gif.Equals(img.RawFormat))
            {
                try
                {
                    NumFrames = img.GetFrameCount(FrameDimension.Time);
                    IsSimplePng = false;
                    DefaultImage._image = new Bitmap(img);
                    for (int i=0;i< NumFrames; i++)
                    {
                        Frame f = new Frame();
                        byte[] times = img.GetPropertyItem(0x5100).Value;
                        int dur = BitConverter.ToInt32(times, 4 * i);
                        img.SelectActiveFrame(FrameDimension.Time, i);
                        f._image = img.Clone(new Rectangle(Point.Empty,img.Size),PixelFormat.Format32bppArgb);
                        f._delay = dur / 100.0;
                        _frames.Add(f);
                    }
                }
                catch
                {
                    IsSimplePng = true;
                    DefaultImage._image = img;
                    DefaultImage._delay = 0;
                    NumFrames = 0;
                    _frames.Insert(0, DefaultImage);
                }
                img.Dispose();
                return;
            }

            // if JPEG,... it is a static image
            if (!ImageFormat.Png.Equals(img.RawFormat))
            {
                IsSimplePng = true;
                DefaultImage._image = img;
                DefaultImage._delay = 0;
                NumFrames = 0;
                _frames.Insert(0, DefaultImage);
                return;
            }

            // Finally process APNG files

            // check file signature.
            if (!Helper.IsBytesEqual(ms.ReadBytes(Frame.Signature.Length), Frame.Signature))
                throw new Exception("File signature incorrect.");

            // Read IHDR chunk.
            IhdrChunk = new IhdrChunk(ms);
            if (IhdrChunk.ChunkType != "IHDR")
                throw new Exception("IHDR chunk must located before any other chunks.");

            Bitmap canvas = new Bitmap(IhdrChunk.Width, IhdrChunk.Height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(canvas);

            // Now let's loop in chunks
            try
            {
                Chunk chunk;
                Frame frame = null;
                var otherChunks = new List<OtherChunk>();
                var isIdatAlreadyParsed = false;
                do
                {
                    if (ms.Position == ms.Length)
                        throw new Exception("IEND chunk expected.");

                    chunk = new Chunk(ms);

                    switch (chunk.ChunkType)
                    {
                        case "IHDR":
                            throw new Exception("Only single IHDR is allowed.");
                        // break;

                        // case "bKGD": is not processed

                        case "acTL":
                            if (IsSimplePng)
                                throw new Exception("acTL chunk must located before any IDAT and fdAT");

                            AcTlChunk = new AcTlChunk(chunk);
                            break;

                        case "IDAT":
                            // To be an ApngImage, acTL must located before any IDAT and fdAT.
                            if (AcTlChunk == null)
                                IsSimplePng = true;

                            // Only default image has IDAT.
                            DefaultImage.IhdrChunk = IhdrChunk;
                            DefaultImage.AddIdatChunk(new IdatChunk(chunk));
                            isIdatAlreadyParsed = true;

                            if (DefaultImage.FcTlChunk != null)
                            {
                                _frames.Insert(0, DefaultImage);
                                DefaultImageIsAnimated = true;
                                //graphics.DrawImage(Image.FromStream(DefaultImage.GetStream()), DefaultImage.FcTlChunk.XOffset, DefaultImage.FcTlChunk.YOffset);
                                //DefaultImage._image = (Bitmap)(canvas.Clone());
                            }
                            break;

                        case "fcTL":
                            // Simple PNG should ignore this.
                            if (IsSimplePng)
                                continue;

                            if (frame != null && frame.IdatChunks.Count == 0)
                                throw new Exception("One frame must have only one fcTL chunk.");

                            // IDAT already parsed means this fcTL is used by FRAME IMAGE.
                            if (isIdatAlreadyParsed)
                            {
                                // register current frame object and build a new frame object
                                // for next use
                                if (frame != null)
                                    _frames.Add(frame);
                                frame = new Frame
                                {
                                    IhdrChunk = IhdrChunk,
                                    FcTlChunk = new FcTlChunk(chunk)
                                };
                            }
                            // Otherwise this fcTL is used by the DEFAULT IMAGE.
                            else
                            {
                                DefaultImage.FcTlChunk = new FcTlChunk(chunk);
                            }
                            break;
                        case "fdAT":
                            // Simple PNG should ignore this.
                            if (IsSimplePng)
                                continue;

                            // fdAT is only used by frame image.
                            if (frame == null || frame.FcTlChunk == null)
                                throw new Exception("fcTL chunk expected.");

                            frame.AddIdatChunk(new FdAtChunk(chunk).ToIdatChunk());
                            //graphics.DrawImage(Image.FromStream(frame.GetStream()), DefaultImage.FcTlChunk.XOffset, DefaultImage.FcTlChunk.YOffset);
                            //DefaultImage._image = (Bitmap)(canvas.Clone());
                            break;

                        case "IEND":
                            // register last frame object
                            if (frame != null)
                                _frames.Add(frame);

                            if (DefaultImage.IdatChunks.Count != 0)
                                DefaultImage.IendChunk = new IendChunk(chunk);
                            foreach (var f in _frames)
                            {
                                f.IendChunk = new IendChunk(chunk);
                            }
                            break;

                        default:
                            otherChunks.Add(new OtherChunk(chunk));
                            break;
                    }
                } while (chunk.ChunkType != "IEND");

                try
                {
                    DefaultImage.GetImage(); // to force Default._image generation
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error loading Png as Apng? : {e.Message}");
                    IsSimplePng = true;
                    DefaultImage._image = img;
                    DefaultImage._delay = 0;
                    NumFrames = 0;
                    _frames.Insert(0, DefaultImage);
                    return;
                }

                // Now we should apply every chunk in otherChunks to every frame.
                //_frames.ForEach(f => otherChunks.ForEach(f.AddOtherChunk));

                NumFrames = _frames.Count;
                for (int j = 0; j < NumFrames; j++)
                {
                    otherChunks.ForEach(_frames[j].AddOtherChunk);

                    if (_frames[j].FcTlChunk.BlendOp == BlendOps.ApngBlendOpSource)
                        graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                    else
                        graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                    graphics.DrawImage(Image.FromStream(_frames[j].GetStream()), _frames[j].FcTlChunk.XOffset, _frames[j].FcTlChunk.YOffset);
                    _frames[j]._image = (Bitmap)(canvas.Clone());
                    if (_frames[j].FcTlChunk.DisposeOp == DisposeOps.ApngDisposeOpBackground)
                        graphics.Clear(Color.Transparent);
                    else if (_frames[j].FcTlChunk.DisposeOp == DisposeOps.ApngDisposeOpPrevious && j > 0)
                    {
                        graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                        graphics.DrawImage(_frames[j - 1]._image, Point.Empty);
                    }
                }

            }
            finally
            {
                canvas?.Dispose();
                graphics?.Dispose();
            }
        }

        /// <summary>
        ///     Indicate whether the file is a simple PNG.
        /// </summary>
        public bool IsSimplePng { get; private set; }

        public bool IsAnimated()
        {
            return !IsSimplePng;
        } 

        /// <summary>
        ///     Indicate whether the default image is part of the animation
        /// </summary>
        public bool DefaultImageIsAnimated { get; private set; }

        /// <summary>
        ///     Gets the base image.
        ///     If IsSimplePNG = True, returns the only image;
        ///     if False, returns the default image
        /// </summary>
        public Frame DefaultImage { get; } = new Frame();

        /// <summary>
        ///     Gets the frame array.
        ///     If IsSimplePNG = True, returns empty
        /// </summary>
        public Frame[] Frames => _frames.ToArray();

        /// <summary>
        ///     Gets the IHDR Chunk
        /// </summary>
        public IhdrChunk IhdrChunk { get; private set; }

        /// <summary>
        ///     Gets the acTL Chunk
        /// </summary>
        public AcTlChunk AcTlChunk { get; private set; }

        /// <summary>
        ///     Gets Number of Frames in an efficient way;
        /// </summary>
        public int NumFrames{ get; private set; }
    }

}
