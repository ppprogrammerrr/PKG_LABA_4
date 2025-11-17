using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _4._Resterization;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private int currentGridSize = 10;
    private Dictionary<Canvas, Rectangle[,]> gridsArray = new Dictionary<Canvas, Rectangle[,]>();
    private string selectedAlgorithm = "Linear";

    public MainWindow()
    {
        InitializeComponent();
        GenerateAllGrids(10);
    }

    private void SelectAlgorithm(object sender, RoutedEventArgs args)
    {
        if (sender is MenuItem m)
        {
                selectedAlgorithm = m.Name.ToString();
                algorithm.Text = selectedAlgorithm;
        }
    }

    private void GenerateGridButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(GridSizeTextBox.Text, out int gridSize) && gridSize > 0)
        {
            currentGridSize = gridSize;
            GenerateAllGrids(gridSize);
        }
        else
        {
            MessageBox.Show("Grid size invalid");
        }
    }

    private void DrawButton_Click(object sender, RoutedEventArgs args)
    {
        if (!int.TryParse(X0TextBox.Text, out int x0) || !int.TryParse(Y0TextBox.Text, out int y0) ||
            !int.TryParse(X1TextBox.Text, out int x1) || !int.TryParse(Y1TextBox.Text, out int y1))
        {
            MessageBox.Show("Coordinates invalid (<0)");
            return;
        }

        if (!int.TryParse(RadiusTextBox.Text, out int radius) || radius < 0)
        {
            MessageBox.Show("Radius invalid");
            return;
        }

        if (x0 < 0 || x0 >= currentGridSize || y0 < 0 || y0 >= currentGridSize ||
            x1 < 0 || x1 >= currentGridSize || y1 < 0 || y1 >= currentGridSize)
        {
            MessageBox.Show($"Coordinates are >= 0 and <= {currentGridSize - 1}");
            return;
        }

        int centerX = currentGridSize / 2;
        int centerY = currentGridSize / 2;
        int maxRadius = Math.Min(centerX, centerY);
        if (radius > maxRadius)
        {
            MessageBox.Show($"Radius is <= {maxRadius}");
            return;
        }

        GenerateAllGrids(currentGridSize);
        switch (selectedAlgorithm) {
            case ("Linear"):
                ExecuteLinearAlgorithm(x0, y0, x1, y1);
                break;
            case ("DDA"):
                ExecuteDDA(x0, y0, x1, y1);
                break;
            case ("Bresenham"):
                 ExecuteBresenham(x0, y0, x1, y1);
                break;
            case ("Circle"):
                ExecuteBresenhamCircle(radius);
                break;
    }
    }

    public void GenerateAllGrids(int gridSize)
    {
        currentGridSize = gridSize;
        GenerateGrid(Canvas, gridSize);
    }

    public void GenerateGrid(Canvas canvas, int gridSize)
    {
        canvas.Children.Clear();
        Rectangle[,] grid = new Rectangle[gridSize, gridSize];

        double canvasSize = canvas.Width;
        double cellSize = canvasSize / gridSize;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                Rectangle rect = new Rectangle
                {
                    Width = cellSize,
                    Height = cellSize,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0,
                    Fill = Brushes.White,
                    Tag = (col, row)  //(x,y), or rather (x,-y)
                };
                if (cellSize > 4)
                    rect.StrokeThickness = 0.5;
                else
                    rect.StrokeThickness = 0.1;
                    Canvas.SetLeft(rect, col * cellSize);
                // flip y
                Canvas.SetTop(rect, (gridSize - 1 - row) * cellSize);

                canvas.Children.Add(rect);
                grid[col, row] = rect; 
            }
        }

        gridsArray[canvas] = grid;
    }

    private void ExecuteLinearAlgorithm(int x0, int y0, int x1, int y1)
    {
        float x1_f = x0;
        float y1_f = y0;
        float x2_f = x1;
        float y2_f = y1;

        // y=kx+b, find k and b primitively
        float k = (y2_f - y1_f) / (x2_f - x1_f);
        float b = y2_f - k * x2_f;

        // step size
        float dx = Math.Abs(x2_f - x1_f) / (Math.Max(Math.Abs(x2_f - x1_f), Math.Abs(y2_f - y1_f)) * 2);
        if (x2_f < x1_f)    //shouldn't happen
        {
            dx = -dx;
        }

        float x = x1_f;
        while (x <= x2_f)
        {
            float y = k * x + b;
            int pixelX = (int)Math.Round(x);
            int pixelY = (int)Math.Round(y);

            if (pixelX >= 0 && pixelX < currentGridSize && pixelY >= 0 && pixelY < currentGridSize) //if in grid
            {
                ColorPixel(Canvas, pixelX, pixelY);
            }

            x += dx;
        }
    }

    private void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    private void ExecuteDDA(int x0, int y0, int x1, int y1)
    {
        float dx = x1 - x0;
        float dy = y1 - y0;


        float steps = Math.Max(Math.Abs(dx), Math.Abs(dy)) * 10;    //more intermediate points for truer dda

        float xInc = dx / steps;
        float yInc = dy / steps;

        float x = x0;
        float y = y0;

        for (int i = 0; i <= steps; i++)
        {
            ColorPixel(Canvas, (int)Math.Round(x), (int)Math.Round(y));
            x += xInc;
            y += yInc;
        }
    }


    private void ExecuteBresenham(int x0, int y0, int x1, int y1)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        int x = x0;
        int y = y0;

        while (true)
        {
            ColorPixel(Canvas, x, y);

            if (x == x1 && y == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private void ExecuteBresenhamCircle(int radius)
    {
        int centerX = currentGridSize / 2;
        int centerY = currentGridSize / 2;

        int x = 0;
        int y = radius;
        int d = 3 - 2 * radius;

        while (y >= x)
        {
            ColorPixel(Canvas, centerX + x, centerY + y);
            ColorPixel(Canvas, centerX - x, centerY + y);
            ColorPixel(Canvas, centerX + x, centerY - y);
            ColorPixel(Canvas, centerX - x, centerY - y);
            ColorPixel(Canvas, centerX + y, centerY + x);
            ColorPixel(Canvas, centerX - y, centerY + x);
            ColorPixel(Canvas, centerX + y, centerY - x);
            ColorPixel(Canvas, centerX - y, centerY - x);
                
            x++;
            if (d > 0)
            {
                y--;
                d += 4 * (x - y) + 10;
            }
            else
            {
                d += 4 * x + 6;
            }
        }
    }

    private void ColorPixel(Canvas canvas, int x, int y)
    {
        if (x < 0 || x >= currentGridSize || y < 0 || y >= currentGridSize)
            return;

        if (gridsArray.ContainsKey(canvas) && gridsArray[canvas][x, y] != null)
        {
            gridsArray[canvas][x, y].Fill = Brushes.Black;
        }
    }
}