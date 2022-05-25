/**
 * The $P Point-Cloud Recognizer (.NET Framework C# version)
 *
 * 	    Radu-Daniel Vatavu, Ph.D.
 *	    University Stefan cel Mare of Suceava
 *	    Suceava 720229, Romania
 *	    vatavu@eed.usv.ro
 *
 *	    Lisa Anthony, Ph.D.
 *      UMBC
 *      Information Systems Department
 *      1000 Hilltop Circle
 *      Baltimore, MD 21250
 *      lanthony@umbc.edu
 *
 *	    Jacob O. Wobbrock, Ph.D.
 * 	    The Information School
 *	    University of Washington
 *	    Seattle, WA 98195-2840
 *	    wobbrock@uw.edu
 *
 * The academic publication for the $P recognizer, and what should be 
 * used to cite it, is:
 *
 *	Vatavu, R.-D., Anthony, L. and Wobbrock, J.O. (2012).  
 *	  Gestures as point clouds: A $P recognizer for user interface 
 *	  prototypes. Proceedings of the ACM Int'l Conference on  
 *	  Multimodal Interfaces (ICMI '12). Santa Monica, California  
 *	  (October 22-26, 2012). New York: ACM Press, pp. 273-280.
 *
 * This software is distributed under the "New BSD License" agreement:
 *
 * Copyright (c) 2012, Radu-Daniel Vatavu, Lisa Anthony, and 
 * Jacob O. Wobbrock. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the names of the University Stefan cel Mare of Suceava, 
 *	    University of Washington, nor UMBC, nor the names of its contributors 
 *	    may be used to endorse or promote products derived from this software 
 *	    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Radu-Daniel Vatavu OR Lisa Anthony
 * OR Jacob O. Wobbrock BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
**/
using System;
using UnityEngine;

namespace PDollarGestureRecognizer
{
    /// <summary>
    /// Implements a gesture as a cloud of points (i.e., an unordered set of points).
    /// For $P, gestures are normalized with respect to scale, translated to origin, and resampled into a fixed number of 32 points.
    /// For $Q, a LUT is also computed.
    /// </summary>
    [Serializable]
    public class Gesture
    {
        public Point[] Points = null;            // gesture points (normalized)
        public Point[] PointsRaw = null;         // gesture points (not normalized, as captured from the input device)
        public string Name = "";                 // gesture class
        
        public static int SAMPLING_RESOLUTION = 64;                             // default number of points on the gesture path
        private const int MAX_INT_COORDINATES = 1024;                           // $Q only: each point has two additional x and y integer coordinates in the interval [0..MAX_INT_COORDINATES-1] used to operate the LUT table efficiently (O(1))
        public static int LUT_SIZE = 64;                                        // $Q only: the default size of the lookup table is 64 x 64
        public static int LUT_SCALE_FACTOR = MAX_INT_COORDINATES / LUT_SIZE;    // $Q only: scale factor to convert between integer x and y coordinates and the size of the LUT

        [HideInInspector] public int[][] LUT = null;               // lookup table

        /// <summary>
        /// Constructs a gesture from an array of points
        /// </summary>
        /// <param name="points"></param>
        public Gesture(Point[] points, string gestureName = "")
        {
            this.Name = gestureName;

            this.PointsRaw = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
                this.PointsRaw[i] = new Point(points[i].pos, points[i].StrokeID);//

            this.Normalize();
        }

        /// <summary>
        /// Normalizes the gesture path. 
        /// The $Q recognizer requires an extra normalization step, the computation of the LUT, 
        /// which can be enabled with the computeLUT parameter.
        /// </summary>
        public void Normalize(bool computeLUT = true)
        {
            // standard $-family processing: resample, scale, and translate to origin
            this.Points = Resample(PointsRaw, SAMPLING_RESOLUTION);
            this.Points = Scale(Points);
            this.Points = TranslateTo(Points, Centroid(Points));
            
            if (computeLUT) // constructs a lookup table for fast lower bounding (used by $Q)
            {
                this.TransformCoordinatesToIntegers();
                this.ConstructLUT();
            }
        }

        #region gesture pre-processing steps: scale normalization, translation to origin, and resampling

        /// <summary>
        /// Performs scale normalization with shape preservation into [0..1]x[0..1]
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private Point[] Scale(Point[] points)
        {
            int nPoints = points.Length; //

            float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue,//
                  maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;//
            for (int i = 0; i < nPoints; i++)
            {
                if (minx > points[i].pos.x) minx = points[i].pos.x;
                if (miny > points[i].pos.y) miny = points[i].pos.y;
                if (minz > points[i].pos.z) minz = points[i].pos.z;//
                if (maxx < points[i].pos.x) maxx = points[i].pos.x;
                if (maxy < points[i].pos.y) maxy = points[i].pos.y;
                if (maxz < points[i].pos.z) maxz = points[i].pos.z;//
            }

            Point[] newPoints = new Point[nPoints];
            float scale = Math.Max(maxx - minx, maxy - miny);
            scale = Math.Max(scale, maxz - minz);//
            
            for (int i = 0; i < nPoints; i++)
            {
                float x = points[i].pos.x;
                newPoints[i] = new Point((x - minx) / scale, (x - miny) / scale,
                                          (x - minz) / scale, points[i].StrokeID);//
            }
            return newPoints;
        }

        /// <summary>
        /// Translates the array of points by p
        /// </summary>
        /// <param name="points"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private Point[] TranslateTo(Point[] points, Point p)
        {
            int nPoints = points.Length; //
            Point[] newPoints = new Point[nPoints];
            for (int i = 0; i < nPoints; i++)
                 newPoints[i] = new Point(points[i].pos - p.pos, points[i].StrokeID);//
            //newPoints[i] = new Point(points[i].pos.x - p.pos.x, points[i].pos.y - p.pos.y, points[i].StrokeID);

            return newPoints;
        }

        /// <summary>
        /// Computes the centroid for an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private Point Centroid(Point[] points)
        {
            int nPoints = points.Length;//
            //float cx = 0, cy = 0;
            Vector3 c = new Vector3();
            for (int i = 0; i < nPoints; i++)
            {
                c += points[i].pos;//
                //cx += points[i].pos.x;
                //cy += points[i].pos.y;
            }
            return new Point(c / nPoints , 0); //
        }

        /// <summary>
        /// Resamples the array of points into n equally-distanced points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public Point[] Resample(Point[] points, int n)
        {
            Point[] newPoints = new Point[n];
            newPoints[0] = new Point(points[0].pos, points[0].StrokeID); //
            int numPoints = 1;
            int pointsLength = points.Length;

            float I = PathLength(points) / (n - 1); // computes interval length
            float D = 0;
            for (int i = 1; i < pointsLength; i++)
            {
                if (points[i].StrokeID == points[i - 1].StrokeID)
                {
                    float d = Geometry.EuclideanDistance(points[i - 1], points[i]);
                    if (D + d >= I)
                    {
                        Point firstPoint = points[i - 1];
                        while (D + d >= I)
                        {
                            // add interpolated point
                            float t = Math.Min(Math.Max((I - D) / d, 0.0f), 1.0f);
                            if (float.IsNaN(t)) t = 0.5f;

                            Vector3 aa = points[i].pos;
                            int sID = points[i].StrokeID;
                            newPoints[numPoints++] = new Point( (1.0f - t) * firstPoint.pos + t * aa, sID); //

                            // update partial length
                            d = D + d - I;
                            D = 0;
                            firstPoint = newPoints[numPoints - 1];
                        }
                        D = d;
                    }
                    else D += d;
                }
            }

            if (numPoints == n - 1) // sometimes we fall a rounding-error short of adding the last point, so add it if so
                newPoints[numPoints++] = new Point(points[pointsLength - 1].pos, points[pointsLength - 1].StrokeID); //
            return newPoints;
        }

        /// <summary>
        /// Computes the path length for an array of points
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private float PathLength(Point[] points)
        {
            float length = 0;
            for (int i = 1; i < points.Length; i++)
                if (points[i].StrokeID == points[i - 1].StrokeID)
                    length += Geometry.EuclideanDistance(points[i - 1], points[i]);
            return length;
        }

        /// <summary>
        /// Scales point coordinates to the integer domain [0..MAXINT-1] x [0..MAXINT-1]
        /// </summary>
        private void TransformCoordinatesToIntegers()
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i].intX = (int)((Points[i].pos.x + 1.0f) / 2.0f * (MAX_INT_COORDINATES - 1));
                Points[i].intY = (int)((Points[i].pos.y + 1.0f) / 2.0f * (MAX_INT_COORDINATES - 1));
                Points[i].intZ = (int)((Points[i].pos.z + 1.0f) / 2.0f * (MAX_INT_COORDINATES - 1));
            }
        }

        /// <summary>
        /// Constructs a Lookup Table that maps grip points to the closest point from the gesture path
        /// </summary>
        private void ConstructLUT()
        {
            this.LUT = new int[LUT_SIZE][];
            for (int i = 0; i < LUT_SIZE; i++)
                LUT[i] = new int[LUT_SIZE];

            for (int i = 0; i < LUT_SIZE; i++)
                for (int j = 0; j < LUT_SIZE; j++)
                {
                    int minDistance = int.MaxValue;
                    int indexMin = -1;
                    for (int t = 0; t < Points.Length; t++)
                    {
                        int row = Points[t].intY / LUT_SCALE_FACTOR;
                        int col = Points[t].intX / LUT_SCALE_FACTOR;
                        int dist = (row - i) * (row - i) + (col - j) * (col - j);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            indexMin = t;
                        }
                    }
                    LUT[i][j] = indexMin;
                }
        }

        /// <summary>
        /// Constructs a Lookup Table that maps grip points to the closest point from the gesture path
        /// Versión 3D para que tenga en cuenta el eje Z. (WIP)
        /// </summary>
        private void ConstructLUT3D()
        {
            this.LUT = new int[LUT_SIZE][];
            for (int i = 0; i < LUT_SIZE; i++)
                LUT[i] = new int[LUT_SIZE];

            for (int i = 0; i < LUT_SIZE; i++)
                for (int j = 0; j < LUT_SIZE; j++)
                {
                    int minDistance = int.MaxValue;
                    int indexMin = -1;
                    for (int t = 0; t < Points.Length; t++)
                    {
                        int row = Points[t].intY / LUT_SCALE_FACTOR;
                        int col = Points[t].intX / LUT_SCALE_FACTOR;
                        int dist = (row - i) * (row - i) + (col - j) * (col - j);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            indexMin = t;
                        }
                    }
                    LUT[i][j] = indexMin;
                }
        }

        #endregion
    }
}