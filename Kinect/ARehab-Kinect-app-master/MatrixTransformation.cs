using System;
using MathNet;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace Csharp_3d_viewer
{

    public class MatrixTransformation
    {
        Matrix<float> A, A_m;
        Matrix<float> B, B_m;

        Matrix<float> R;
        Vector<float> t;

        Vector<float> Centroid_A;
        Vector<float> Centroid_B;

        Vector<float> v_in, v_out;
        /* 
         * Reference
         * 
         * % This function finds the optimal Rigid/Euclidean transform in 3D space
    % It expects as input a 3xN matrix of 3D points.
    % It returns R, t

    % expects row data
    function [R,t] = rigid_transform_3D(A, B)
        if nargin != 2
            error("Missing parameters");
        end

        assert(size(A) == size(B));

        [num_rows, num_cols] = size(A);
        if num_rows != 3
            error("matrix A is not 3xN, it is %dx%d", num_rows, num_cols)
        end

        [num_rows, num_cols] = size(B);
        if num_rows != 3
            error("matrix B is not 3xN, it is %dx%d", num_rows, num_cols)
        end

        % find mean column wise
        centroid_A = mean(A, 2);
        centroid_B = mean(B, 2);

        % subtract mean
        Am = A - repmat(centroid_A, 1, num_cols);
        Bm = B - repmat(centroid_B, 1, num_cols);

        % calculate covariance matrix (is this the corrcet term?)
        H = Am * B';

        % find rotation
        [U,S,V] = svd(H);
        R = V*U';

        if det(R) < 0
            printf("det(R) < R, reflection detected!, correcting for it ...\n");
            V(:,3) *= -1;
            R = V*U';
        end

        t = -R*centroid_A + centroid_B;
    end
         * 
         * 
         * 
         * 
         * 
         * 
         * 
         * */

        public MatrixTransformation()
        {
            R = Matrix<float>.Build.Dense(3, 3, 0);
            R[0, 0] = 1.0f;
            R[1, 1] = 1.0f;
            R[2, 2] = 1.0f;

            v_in = Vector<float>.Build.Dense(3, 0);
            v_out = Vector<float>.Build.Dense(3, 0);

            t = Vector<float>.Build.Dense(3, 0);
        }


        public void Transform(float x, float y, float z, out float x_out, out float y_out, out float z_out)
        {

            v_in[0] = x;
            v_in[1] = y;
            v_in[2] = z;

            //v_out = 

            x_out = 0;
            y_out = 0;
            z_out = 0;
        
        
        
        }

        /*
        public void Transform(Point3D p_in, out Point3D p_out)
        {
            //Point3D p = new Point3D
            p_out
            p_out.X = 0;
            p_out.Y = 0;
            p_out.Z = 0;


        }
        */



        public void OldConstructor()
        {

            int a = 3;
            int b = 10;

            A = Matrix<float>.Build.Random(a, b);

            float[,] MArray = { { 0, -1.0f, 0 }, { 0, 0, 1.0f }, { 1.0f, 0, 0 } };
            float[] MVector = { 100f, -10f, 40f };

            Matrix<float> R1 = Matrix<float>.Build.DenseOfArray(MArray);
            Vector<float> T1 = Vector<float>.Build.Dense(MVector);

            B = R1.Multiply(A);

            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    B[i, j] = B[i, j] + T1[i];
                }
            }

            A_m = Matrix<float>.Build.Dense(a, b, 0);
            B_m = Matrix<float>.Build.Random(a, b, 0);

            Centroid_A = Vector<float>.Build.Random(a);
            Centroid_B = Vector<float>.Build.Random(a);

            for (int i = 0; i < a; i++)
            {
                Centroid_A[i] = 0;
                Centroid_B[i] = 0;

                for (int j = 0; j < b; j++)
                {
                    Centroid_A[i] += A[i, j];
                    Centroid_B[i] += B[i, j];
                }

                Centroid_A[i] /= (float)b;
                Centroid_B[i] /= (float)b;
            }

            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    A_m[i, j] = A[i, j] - Centroid_A[i];
                    B_m[i, j] = B[i, j] - Centroid_B[i];
                }
            }

            //    % calculate covariance matrix (is this the corrcet term?)
            //H = Am * B';
            Matrix<float> H = A_m.Multiply(B.Transpose());

            MathNet.Numerics.LinearAlgebra.Factorization.Svd<float> svd = H.Svd(true);

            //% find rotation
            //[U, S, V] = svd(H);
            //R = V * U';

            Matrix<float> R = svd.VT.Transpose().Multiply(svd.U.Transpose());

            Vector<float> t = -R * Centroid_A + Centroid_B;

            Matrix<float> C = R.Multiply(A);

            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    C[i, j] = C[i, j] + t[i];
                }
            }

            Console.WriteLine(R.ToString());
            Console.WriteLine(t.ToString());

            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < a; j++)
                {
                    Console.WriteLine((R1[i, j] - R[i, j]).ToString("0.0000"));
                }
            }

        }

        
        public void UpdateTransformation(List<Point3D> Kinect_LH, List<Point3D> Kinect_RH, List<Point3D> Hololens_LH, List<Point3D> Hololens_RH)
        {
            int size = Kinect_LH.Count * 2;

            Matrix<float> A = Matrix<float>.Build.Dense(3, size, 0);
            Matrix<float> B = Matrix<float>.Build.Dense(3, size, 0);

            //let's load the matrixes
            for (int j = 0; j < size; j+=2)
            {
                A[0, j] = Kinect_LH[j / 2].X;
                A[1, j] = Kinect_LH[j / 2].Y;
                A[2, j] = Kinect_LH[j / 2].Z;

                A[0, j+1] = Kinect_RH[j / 2].X;
                A[1, j+1] = Kinect_RH[j / 2].Y;
                A[2, j+1] = Kinect_RH[j / 2].Z;

                B[0, j] = Hololens_LH[j / 2].X;
                B[1, j] = Hololens_LH[j / 2].Y;
                B[2, j] = Hololens_LH[j / 2].Z;

                B[0, j + 1] = Hololens_RH[j / 2].X;
                B[1, j + 1] = Hololens_RH[j / 2].Y;
                B[2, j + 1] = Hololens_RH[j / 2].Z;
            }

            //R1 = Matrix<float>.Build.Dense(3,3,0);
            //T1 = Vector<float>.Build.Dense(3,0);


            A_m = Matrix<float>.Build.Dense(3, size, 0);
            B_m = Matrix<float>.Build.Random(3, size, 0);

            Centroid_A = Vector<float>.Build.Random(3);
            Centroid_B = Vector<float>.Build.Random(3);

            for (int i = 0; i < 3; i++)
            {
                Centroid_A[i] = 0;
                Centroid_B[i] = 0;

                for (int j = 0; j < size; j++)
                {
                    Centroid_A[i] += A[i, j];
                    Centroid_B[i] += B[i, j];
                }

                Centroid_A[i] /= (float)size;
                Centroid_B[i] /= (float)size;
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    A_m[i, j] = A[i, j] - Centroid_A[i];
                    B_m[i, j] = B[i, j] - Centroid_B[i];
                }
            }

            //    % calculate covariance matrix (is this the corrcet term?)
            //H = Am * B';
            Matrix<float> H = A_m.Multiply(B.Transpose());

            MathNet.Numerics.LinearAlgebra.Factorization.Svd<float> svd = H.Svd(true);

            //% find rotation
            //[U, S, V] = svd(H);
            //R = V * U';

            //Matrix<float> R = svd.VT.Transpose().Multiply(svd.U.Transpose());
            R = svd.VT.Transpose().Multiply(svd.U.Transpose());
            t = -R * Centroid_A + Centroid_B;

        }
        
        }





        public class Point3D
        {
            public float X, Y, Z;

            public Point3D(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }


        public class PointArray
        {
            private float[] X, Y, Z;
            public float avg_x, avg_y, avg_z;

            int counter = 0;
            int size = 0;

            public PointArray(int _size)
            {
                X = new float[_size];
                Y = new float[_size];
                Z = new float[_size];

                size = _size;
            }

            public void Reset()
            {
                for (int i = 0; i < size; i++)
                {
                    X[i] = 0;
                    Y[i] = 0;
                    Z[i] = 0;
                }

                counter = 0;
            }


            public void AddPoint(float x, float y, float z)
            {
                X[counter] = x;
                Y[counter] = y;
                Z[counter] = z;

                counter++;

                if (counter >= size)
                    counter = 0;
            }

            public void UpdateAverages(int start_perc, int stop_perc)
            {
                float temp_x = 0, temp_y = 0, temp_z = 0;
                int start_counter = (int)((float)start_perc / 100.0f * (float)counter);
                int stop_counter = (int)((float)stop_perc / 100.0f * (float)counter);

                int avg_size = stop_counter - start_counter;


                for (int i = start_counter; i < stop_counter; i++)
                {
                    temp_x += X[i];
                    temp_y += Y[i];
                    temp_z += Z[i];
                }

                avg_x = temp_x / (float)avg_size;
                avg_y = temp_y / (float)avg_size;
                avg_z = temp_z / (float)avg_size;

            }




        }
}