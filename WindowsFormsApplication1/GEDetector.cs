using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace WindowsFormsApplication1
{
    class GEDetector
    {
        #region Declaration
        // There is a general definition of Gesture Positions that is in common between all of the gestures.
        private enum GesturePosition
        {
            None = 0,
            Start = 1,
            Middle = 2,
            End = 3
        }
        // There is a general definition for Gesture States that is in common between all of the gestures.
        private enum GestureState
        {
            None = 0,
            Success = 1,
            InProgress = 2,
            Failure = 3
        }

        private const int timeOut = 3000; // This is the time out delay for cancelling the gesture detection process.
        private const int leftHand = 0;
        private const int rightHand = 1;
        private const int generalItrations = 2; // Each gesture needs a number of iterations to be counted before a successful finish. This is the general number.
        private const int waveItrations = 3; // This is the specefic number of iterations for Wave.
        private const float threshold = 0.1f; // This system needs a general threshold for comparing of the joints positions. 
        #endregion Declaration

        // This structure is used to define and store each gesture.
        // Each gesture type has an array of this structure to store the state of each hand or leg.
        #region GestureTracker Structure
        private struct GestureTracker
        {
            #region FieldMembers
            public long timeStamp; // This field stores the time of the latest update of the gesture.
            public int counter;
            public GestureState state;
            public GesturePosition startPosition; // We need to know the start position of the gesture so we keep it at this feild.
            public GesturePosition currentPosition;
            #endregion FieldMembers

            #region Methods
            public void Reset()
            {
                this.timeStamp = 0;
                this.counter = 0;
                this.state = GestureState.None;
                this.startPosition = GesturePosition.None;
                this.currentPosition = GesturePosition.None;
            }
            public void UpdateState(GestureState state, long timeStamp)
            {
                this.state = state;
                this.timeStamp = timeStamp;
            }
            public void UpdateWave(GesturePosition position, long timeStamp)
            {
                if (this.currentPosition != position)
                {
                    if (position == GesturePosition.Start || position == GesturePosition.End)
                    {
                        if (this.state != GestureState.InProgress)
                        {
                            this.state = GestureState.InProgress;
                            this.counter = 0;
                            this.startPosition = position;
                        }
                        this.counter++;
                    }
                    this.currentPosition = position;
                    this.timeStamp = timeStamp;
                }
            }
            public void UpdatePosition(GesturePosition position, long timeStamp)
            {
                if (this.currentPosition != position)
                {
                    if (position == GesturePosition.Start && this.state != GestureState.InProgress)
                    {
                        this.state = GestureState.InProgress;
                        this.counter = 1;
                        this.startPosition = position;
                    }
                    else if (position == GesturePosition.End && this.state == GestureState.InProgress)
                    {
                        this.counter++;
                    }
                    this.currentPosition = position;
                    this.timeStamp = timeStamp;
                }
            }
            #endregion Methods
        }
        #endregion GestureTracker Structure

        #region Member Variables
        private GestureTracker[,] _WaveTracker = new GestureTracker[6, 2];
        private GestureTracker[,] _SwipeTracker = new GestureTracker[6, 2];
        private GestureTracker[,] _UpDownTracker = new GestureTracker[6, 2];
        private GestureTracker[,] _PunchTracker = new GestureTracker[6, 2];
        private GestureTracker[,] _KickTracker = new GestureTracker[6, 2];
        private GestureTracker[] _StopTracker = new GestureTracker[6];

        private int _GestureCode;
        private int _ResetCounter = 0;
        private bool _Categorized = false;

        public event EventHandler GestureDetected;
        #endregion Member Variables

        #region Methods
        // This function checks the tracking state of each skeleton. 
        // It calls TrackGesture() when it has a tracked skeleton.
        public void Update(Skeleton[] skeletons, long frameTimeStamp)
        {
            if (skeletons != null)
            {
                Skeleton s;
                for (int i = 0; i < skeletons.Length; i++)
                {
                    s = skeletons[i];
                    if (s.TrackingState != SkeletonTrackingState.NotTracked)
                    {
                        TrackGesture(s, i, frameTimeStamp);
                    }
                    else
                    {
                        this.Reset(i);
                    }
                }
            }
        }

        // This is the main function for detecting the gesture.
        // It checks the position of all of the joints and compare them together,
        // then it decides to update which one one of them.
        // If it can not categorized the the position five times, it will reset all of the variables.
        private void TrackGesture(Skeleton skeleton, int index, long timestamp)
        {
            JointType leftHandJointId = JointType.HandLeft;
            JointType rightHandJointId = JointType.HandRight;
            JointType leftWristId = JointType.WristLeft;
            JointType rightWristId = JointType.WristRight;
            JointType leftElbowJointId = JointType.ElbowLeft;
            JointType rightElbowJointId = JointType.ElbowRight;
            JointType leftShoulderId = JointType.ShoulderLeft;
            JointType rightShoulderId = JointType.ShoulderRight;

            JointType leftHipJointId = JointType.HipLeft;
            JointType leftKneeJointId = JointType.KneeLeft;
            JointType leftFootJointId = JointType.FootLeft;
            JointType rightHipJointId = JointType.HipRight;
            JointType rightKneeJointId = JointType.KneeRight;
            JointType rightFootJointId = JointType.FootRight;

            Joint leftHand = skeleton.Joints[leftHandJointId];
            Joint rightHand = skeleton.Joints[rightHandJointId];
            Joint leftWrist = skeleton.Joints[leftWristId];
            Joint rightWrist = skeleton.Joints[rightWristId];
            Joint leftElbow = skeleton.Joints[leftElbowJointId];
            Joint rightElbow = skeleton.Joints[rightElbowJointId];
            Joint leftShoulder = skeleton.Joints[leftShoulderId];
            Joint rightShoulder = skeleton.Joints[rightShoulderId];

            Joint leftHip = skeleton.Joints[leftHipJointId];
            Joint leftKnee = skeleton.Joints[leftKneeJointId];
            Joint leftFoot = skeleton.Joints[leftFootJointId];
            Joint rightHip = skeleton.Joints[rightHipJointId];
            Joint rightKnee = skeleton.Joints[rightKneeJointId];
            Joint rightFoot = skeleton.Joints[rightFootJointId];

            if (leftShoulder.TrackingState != JointTrackingState.NotTracked &&
                rightShoulder.TrackingState != JointTrackingState.NotTracked &&
                leftElbow.TrackingState != JointTrackingState.NotTracked &&
                rightElbow.TrackingState != JointTrackingState.NotTracked &&
                leftWrist.TrackingState != JointTrackingState.NotTracked &&
                rightWrist.TrackingState != JointTrackingState.NotTracked &&
                leftHand.TrackingState != JointTrackingState.NotTracked &&
                rightHand.TrackingState != JointTrackingState.NotTracked)
            {
                // responsible for  PAN LEFT
                #region Left Hand Wave and Punch
                if (leftHand.Position.Y > leftElbow.Position.Y &&
                    leftHand.Position.Y > leftShoulder.Position.Y)
                {
                    this._Categorized = true;

                    // left hand wave
                    if (timestamp > this._WaveTracker[index, 0].timeStamp + timeOut &&
                        this._WaveTracker[index, 0].state == GestureState.InProgress)
                    {
                        this._WaveTracker[index, 0].UpdateState(GestureState.Failure, timestamp);
                    }
                    else if (leftHand.Position.X <= leftElbow.Position.X - threshold)
                    {
                        this._WaveTracker[index, 0].UpdateWave(GesturePosition.Start, timestamp);
                    }
                    else if (leftHand.Position.X >= leftElbow.Position.X + threshold)
                    {
                        this._WaveTracker[index, 0].UpdateWave(GesturePosition.End, timestamp);
                    }
                    if (this._WaveTracker[index, 0].state != GestureState.Success && this._WaveTracker[index, 0].counter == waveItrations)
                    {
                        this._WaveTracker[index, 0].UpdateState(GestureState.Success, timestamp);

                        if (GestureDetected != null)
                        {
                            this.Gesture = 1;
                            GestureDetected(this, new EventArgs());
                            this.Reset(index);
                        }
                    }

                    // Left Hand Punch
                    if (timestamp > this._PunchTracker[index, 0].timeStamp + timeOut)
                    //this._PunchTracker[index, 0].state == GestureState.InProgress)
                    {
                        this._PunchTracker[index, 0].UpdateState(GestureState.Failure, timestamp);
                    }
                    else if (//leftHand.Position.X <= leftElbow.Position.X && // - threshold &&
                             Math.Abs(leftHand.Position.Z - leftShoulder.Position.Z) <= 2.5 * threshold &&
                             this._PunchTracker[index, 0].state != GestureState.InProgress)
                    {
                        this._PunchTracker[index, 0].UpdatePosition(GesturePosition.Start, timestamp);
                    }
                    else if (//leftHand.Position.X <= leftElbow.Position.X &&// - threshold &&
                             Math.Abs(leftHand.Position.Z - leftShoulder.Position.Z) >= 3.5 * threshold &&
                             this._PunchTracker[index, 0].state == GestureState.InProgress)
                    {
                        this._PunchTracker[index, 0].UpdatePosition(GesturePosition.End, timestamp);
                    }

                    if (this._PunchTracker[index, 0].state != GestureState.Success && this._PunchTracker[index, 0].counter == generalItrations)
                    {
                        this._PunchTracker[index, 0].UpdateState(GestureState.Success, timestamp);

                        if (GestureDetected != null)
                        {
                            this.Gesture = 2;
                            GestureDetected(this, new EventArgs());
                            this.Reset(index);
                        }
                    }

                   
                    if (leftHand.Position.Y > leftElbow.Position.Y + threshold &&
                        this._UpDownTracker[index, 0].state != GestureState.InProgress)
                    {
                        this._UpDownTracker[index, 0].UpdatePosition(GesturePosition.Start, timestamp);
                    }
                }
                #endregion Left Hand Wave and Punch

                // This part is responsible for right hand wave and punch compeletly and for the end position of turn up volume.                
                #region Right Hand Wave and Punch
                if (rightHand.Position.Y > rightElbow.Position.Y &&
                    rightHand.Position.Y > rightShoulder.Position.Y)
                {
                    this._Categorized = true;
                    // right hand wave
                    if (timestamp > this._WaveTracker[index, 1].timeStamp + timeOut &&
                        this._WaveTracker[index, 1].state == GestureState.InProgress)
                    {
                        this._WaveTracker[index, 1].UpdateState(GestureState.Failure, timestamp);
                    }
                    else if (rightHand.Position.X >= rightElbow.Position.X + threshold)
                    {
                        this._WaveTracker[index, 1].UpdateWave(GesturePosition.Start, timestamp);
                    }
                    else if (rightHand.Position.X <= rightElbow.Position.X - threshold)
                    {
                        this._WaveTracker[index, 1].UpdateWave(GesturePosition.End, timestamp);
                    }

                    if (this._WaveTracker[index, 1].state != GestureState.Success && this._WaveTracker[index, 1].counter == waveItrations)
                    {
                        this._WaveTracker[index, 1].UpdateState(GestureState.Success, timestamp);

                        if (GestureDetected != null)
                        {
                            this.Gesture = 3;
                            GestureDetected(this, new EventArgs());
                            this.Reset(index);
                        }
                    }
                    // Right Hand Punch
                    if (timestamp > this._PunchTracker[index, 1].timeStamp + timeOut)
                    //this._PunchTracker[index, 1].state == GestureState.InProgress)
                    {
                        this._PunchTracker[index, 1].UpdateState(GestureState.Failure, timestamp);
                    }
                    else if (//rightHand.Position.X <= rightElbow.Position.X && // - threshold &&
                             Math.Abs(rightHand.Position.Z - rightShoulder.Position.Z) <= 2.5 * threshold &&
                             this._PunchTracker[index, 1].state != GestureState.InProgress)
                    {
                        this._PunchTracker[index, 1].UpdatePosition(GesturePosition.Start, timestamp);
                    }
                    else if (//rightHand.Position.X <= leftElbow.Position.X &&// - threshold &&
                             Math.Abs(rightHand.Position.Z - rightShoulder.Position.Z) >= 3.5 * threshold &&
                             this._PunchTracker[index, 1].state == GestureState.InProgress)
                    {
                        this._PunchTracker[index, 1].UpdatePosition(GesturePosition.End, timestamp);
                    }

                    if (this._PunchTracker[index, 1].state != GestureState.Success && this._PunchTracker[index, 1].counter == generalItrations)
                    {
                        this._PunchTracker[index, 1].UpdateState(GestureState.Success, timestamp);

                        if (GestureDetected != null)
                        {
                            this.Gesture = 4;
                            GestureDetected(this, new EventArgs());
                            this.Reset(index);
                        }
                    }

                  
                    
                }
                #endregion Right Hand Wave and Punch

                // This part is responsible for left hand swipe compeletly
                #region Left Hand Swipe
                if (leftShoulder.Position.Y > leftHand.Position.Y &&   // left hand swipe
                         leftHand.Position.Y > leftElbow.Position.Y) //+ threshold)
                {
                    // Left Hand Swipe
                    this._Categorized = true;
                    if (this._SwipeTracker[index, 0].timeStamp + timeOut < timestamp &&
                        this._SwipeTracker[index, 0].state == GestureState.InProgress)
                    {
                        this._SwipeTracker[index, 0].UpdateState(GestureState.Failure, timestamp);
                    }
                    else if (leftHand.Position.X <= leftElbow.Position.X - threshold &&
                             this._SwipeTracker[index, 0].state != GestureState.InProgress)
                    {
                        this._SwipeTracker[index, 0].UpdatePosition(GesturePosition.Start, timestamp);
                    }
                    else if (leftHand.Position.X >= leftElbow.Position.X + threshold &&
                             this._SwipeTracker[index, 0].state == GestureState.InProgress)
                    {
                        this._SwipeTracker[index, 0].UpdatePosition(GesturePosition.End, timestamp);
                    }


                    if (this._SwipeTracker[index, 0].state != GestureState.Success && this._SwipeTracker[index, 0].counter == generalItrations)
                    {
                        this._SwipeTracker[index, 0].UpdateState(GestureState.Success, timestamp);

                        if (GestureDetected != null)
                        {
                            this.Gesture = 3;
                            GestureDetected(this, new EventArgs());
                            this.Reset(index);
                        }
                    }
                }
                #endregion Left Hand Swipe

                // This part is responsible for right hand swipe and Stop compeletly
                #region Right Hand Swipe and Stop
                //Right hand swipe
                if (rightShoulder.Position.Y > rightHand.Position.Y && // right hand swipe 
                         rightHand.Position.Y > rightElbow.Position.Y)// + threshold)
                {
                    this._Categorized = true;
                    if (this._SwipeTracker[index, 1].timeStamp + timeOut < timestamp &&
                        this._SwipeTracker[index, 1].state == GestureState.InProgress)
                    {
                        this._SwipeTracker[index, 1].UpdateState(GestureState.Failure, timestamp);
                    }
                    else if (rightHand.Position.X >= rightElbow.Position.X + threshold &&
                             this._SwipeTracker[index, 1].state != GestureState.InProgress)
                    {
                        this._SwipeTracker[index, 1].UpdatePosition(GesturePosition.Start, timestamp);
                    }
                    else if (rightHand.Position.X <= rightElbow.Position.X - threshold &&
                             this._SwipeTracker[index, 1].state == GestureState.InProgress)
                    {
                        this._SwipeTracker[index, 1].UpdatePosition(GesturePosition.End, timestamp);
                    }


                    if (this._SwipeTracker[index, 1].state != GestureState.Success && this._SwipeTracker[index, 1].counter == generalItrations)
                    {
                        this._SwipeTracker[index, 1].UpdateState(GestureState.Success, timestamp);

                        if (GestureDetected != null)
                        {
                            this.Gesture = 4;
                            GestureDetected(this, new EventArgs());
                            this.Reset(index);
                        }
                    }


                    // Stop
                    if (this._StopTracker[index].timeStamp + timeOut < timestamp)
                    {
                        this._StopTracker[index].UpdateState(GestureState.Failure, timestamp);
                    }
                    else if (this._StopTracker[index].state != GestureState.InProgress)
                    {
                        this._StopTracker[index].UpdatePosition(GesturePosition.Start, timestamp);
                    }
                    else if (this._StopTracker[index].state == GestureState.InProgress &&
                             this._StopTracker[index].currentPosition != GesturePosition.End)
                    {
                        this._StopTracker[index].UpdatePosition(GesturePosition.End, timestamp);
                    }
                    else if (this._StopTracker[index].state == GestureState.InProgress &&
                             this._StopTracker[index].currentPosition != GesturePosition.Middle)
                    {
                        this._StopTracker[index].UpdatePosition(GesturePosition.Middle, timestamp);
                    }
                    if (this._StopTracker[index].state != GestureState.Success && this._StopTracker[index].counter == 90)
                    {
                        this._StopTracker[index].UpdateState(GestureState.Success, timestamp);

                        if (GestureDetected != null)
                        {
                            this.Gesture = 11;
                            GestureDetected(this, new EventArgs());
                            this.Reset(index);
                        }
                    }
                }
                #endregion Right Hand Swipe and Stop

                // This part is responsible for the end part of turn down and the start of turn up.
                #region Up Dwon
                if (leftShoulder.Position.Y > leftElbow.Position.Y &&
                    leftElbow.Position.Y > leftHand.Position.Y)
                {
                    // End for Turn Down volume
                    if (leftHand.Position.X < leftElbow.Position.X)
                    {
                        this._Categorized = true;
                        if (this._UpDownTracker[index, 0].state == GestureState.InProgress)
                        {
                            if (this._UpDownTracker[index, 0].timeStamp + timeOut < timestamp)
                            {
                                this._UpDownTracker[index, 0].UpdateState(GestureState.Failure, timestamp);
                            }
                            else
                            {
                                this._UpDownTracker[index, 0].UpdatePosition(GesturePosition.End, timestamp);
                            }
                        }
                        if (this._UpDownTracker[index, 0].state != GestureState.Success && this._UpDownTracker[index, 0].counter == generalItrations)
                        {
                            this._UpDownTracker[index, 0].UpdateState(GestureState.Success, timestamp);

                            if (GestureDetected != null)
                            {
                                this.Gesture = 5;
                                GestureDetected(this, new EventArgs());
                                this.Reset(index);
                            }
                        }
                    }
                }
                if (rightHand.Position.Y > rightElbow.Position.Y + threshold &&
                        this._UpDownTracker[index, 1].state == GestureState.InProgress)
                {
                    if (this._UpDownTracker[index, 1].timeStamp + timeOut < timestamp)
                    {
                        this._UpDownTracker[index, 1].UpdateState(GestureState.Failure, timestamp);
                    }
                    else
                    {
                        this._UpDownTracker[index, 1].UpdatePosition(GesturePosition.End, timestamp);
                    }
                }
                if (this._UpDownTracker[index, 1].state != GestureState.Success && this._UpDownTracker[index, 1].counter == generalItrations)
                {
                    this._UpDownTracker[index, 1].UpdateState(GestureState.Success, timestamp);

                    if (GestureDetected != null)
                    {
                        this.Gesture = 6;
                        GestureDetected(this, new EventArgs());
                        this.Reset(index);
                    }
                }
                if (rightShoulder.Position.Y > rightElbow.Position.Y &&
                    rightElbow.Position.Y > rightHand.Position.Y)
                {
                    // Start for Turn Up Volume
                    if (rightElbow.Position.X < rightHand.Position.X)
                    {
                        this._Categorized = true;
                        if (this._UpDownTracker[index, 1].timeStamp + timeOut < timestamp)
                        {
                            this._UpDownTracker[index, 1].UpdateState(GestureState.Failure, timestamp);
                        }
                        else if (this._UpDownTracker[index, 1].state != GestureState.InProgress)
                        {
                            this._UpDownTracker[index, 1].UpdatePosition(GesturePosition.Start, timestamp);
                        }
                    }

                }
                #endregion Up Down

                
                
                if (this._Categorized)
                {
                    this._Categorized = false;
                }
                else
                {
                    if (this._ResetCounter < 5)
                    {
                        this._ResetCounter++;
                    }
                    else
                    {
                        this._ResetCounter = 0;
                        this.Reset(index);
                    }
                }
            }
            else
            {
                if (this._ResetCounter < 5)
                {
                    this._ResetCounter++;
                }
                else
                {
                    this._ResetCounter = 0;
                    this.Reset(index);
                }
            }


        }

        private void Reset(int index)
        {
            this._WaveTracker[index, 0].Reset();
            this._WaveTracker[index, 1].Reset();

            this._SwipeTracker[index, 0].Reset();
            this._SwipeTracker[index, 1].Reset();

            this._UpDownTracker[index, 0].Reset();
            this._UpDownTracker[index, 1].Reset();

            this._PunchTracker[index, 0].Reset();
            this._PunchTracker[index, 1].Reset();

            this._KickTracker[index, 0].Reset();
            this._KickTracker[index, 1].Reset();

            this._PunchTracker[index, 0].Reset();
            this._PunchTracker[index, 1].Reset();

            this._StopTracker[index].Reset();
        }
        private double JointDistance(Joint a, Joint b)
        {
            double d = 0;
            Math.Sqrt(Math.Pow(a.Position.X - b.Position.X, 2) + Math.Pow(a.Position.Y - b.Position.Y, 2) + Math.Pow(a.Position.Z - b.Position.Z, 2));
            return d;
        }
        #endregion Methods

        #region Properties
        public int Gesture
        {
            get { return this._GestureCode; }
            set { this._GestureCode = value; }
        }
        #endregion Properties
    }
}
