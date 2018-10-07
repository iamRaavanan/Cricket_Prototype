using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Raavanan
{
    public class MainController : MonoBehaviour
    {
        #region Variables accessed from Inspector
        [SerializeField]
        private Transform mPitchT;
        [SerializeField]
        private GameObject mFreezedPitchGO;
        [SerializeField]
        private Transform mLeftLowT;
        [SerializeField]
        private Transform mRightHighT;
        [SerializeField]
        private Rigidbody mBall;
        [SerializeField]
        private Transform mBattingStumpT;
        [SerializeField]
        private Toggle mBowlingSideToggle;
        [SerializeField]
        private Text mBowlingSideTxt;
        [SerializeField]
        private float mBallBaseSpeed = 75;         // Base Speed 
        [SerializeField]
        private Slider mBallSpeedSlider;
        [SerializeField]
        private Text mBallSpeedMultiplierTxt;
        [SerializeField]
        private float mBatHitPower = 25;
        [SerializeField]
        private GameObject mBowlBtnGO;
        [SerializeField]
        private RectTransform mBowlingSwinRT;
        [SerializeField]
        private GameObject mTutorialGO;
        #endregion
        
        // ---------------- Bowling Variables ----------------
        private bool mIsPithAdjustable = true;
        private bool mIsSwingUpdated = false;
        private Vector2 mPitchLeftMinMax;
        private Vector2 mPitchRightMinMax;
        private Vector3 mPitchPoint;
        private Vector3 mBallInitPosition;
        private Transform mCameraT;
        private float mSwingValue;
        private float mCurrentSpeed;
        private float mBallSpeedMultiplier;

        // ---------------- Batting Variables ----------------
        private bool mIsBattingActive;
        private bool mIsMouseDownPressed;
        private Vector2 mCurrentPosition;
        private Vector2 mPreviousPosition;
        private float mYpos;
        public static MainController _Instance;

        private void Start()
        {
            _Instance = this;
            mBallSpeedMultiplier = (mBallSpeedSlider.value * 10) + 1;
            mBallSpeedMultiplierTxt.text = string.Format("{0:0.0}", mBallSpeedMultiplier);
            mCurrentSpeed = mBallBaseSpeed * mBallSpeedMultiplier;
            //if (PlayerPrefs.GetInt ("Tutorial", 0) == 0)
            {
                mTutorialGO.SetActive(true);
            }
            mBallInitPosition = mBall.transform.position;
            mPitchLeftMinMax = new Vector2(mLeftLowT.position.x, mLeftLowT.position.z);
            mPitchRightMinMax = new Vector2(mRightHighT.position.x, mRightHighT.position.z);
            mCameraT = Camera.main.transform;
        }

        private void Update()
        {
            if (mIsPithAdjustable)
            {
                mPitchT.position += new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * 3 * Time.deltaTime;
                mPitchT.position = new Vector3(Mathf.Clamp(mPitchT.position.x, mPitchLeftMinMax.x, mPitchRightMinMax.x), mPitchT.position.y, Mathf.Clamp(mPitchT.position.z, mPitchRightMinMax.y, mPitchLeftMinMax.y));
            }
            if (mIsBattingActive)
            {
                if (mBall.transform.position.z > mFreezedPitchGO.transform.position.z && !mIsSwingUpdated)
                {
                    mIsSwingUpdated = true;
                    mBall.velocity += (Vector3.right * mSwingValue * 10);
                }
                if (mBall.transform.position.z < mLeftLowT.position.z && mBall.transform.position.z > mRightHighT.position.z)
                {
                    if (Input.GetMouseButtonDown(0) && !mIsMouseDownPressed)
                    {
                        mIsMouseDownPressed = true;
                        mCurrentPosition = mPreviousPosition = Input.mousePosition;
                    }
                    if (Input.GetMouseButtonUp (0) && mIsMouseDownPressed)
                    {
                        mIsMouseDownPressed = false;
                        mCurrentPosition = Input.mousePosition;
                        if (mBall.transform.position.z > -3)
                        {
                            // Perfect Shot
                            mYpos = mBall.velocity.y + 2f;
                        }
                        else
                        {
                            mYpos = mBall.velocity.y;
                        }
                        DetectAndExecutedShot();
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && !mIsMouseDownPressed)
                    {
                        mCurrentPosition = mPreviousPosition = Input.mousePosition;
                    }
                    if (Input.GetMouseButtonUp(0) && mIsMouseDownPressed)
                    {
                        Debug.Log("Swiped Early.");
                    }                    
                }
            }
            mFreezedPitchGO.SetActive(!mIsPithAdjustable);
        }

        /// <summary>
        /// Detect the shot based on swipe and update the velocity of a ball
        /// </summary>
        private void DetectAndExecutedShot()
        {
            Vector3 InDifference = new Vector3(mCurrentPosition.x - mPreviousPosition.x, mYpos, mCurrentPosition.y - mPreviousPosition.y) * mBatHitPower * Time.deltaTime;
            //mBall.AddForce(InDifference);
            mBall.velocity = InDifference;
            mIsBattingActive = false;            
        }

        /// <summary>
        /// Function to reset variables to initial state
        /// </summary>
        /// <returns></returns>
        private IEnumerator ResetAfterDelay()
        {
            yield return new WaitForSeconds(5f);
            ResetVairables();
        }

        /// <summary>
        /// Function to initiate bowing
        /// </summary>
        public void StartBowling ()
        {
            mBowlBtnGO.SetActive(false);
            mPitchPoint = new Vector3(mPitchT.position.x, 0, mPitchT.position.z);
            mFreezedPitchGO.transform.position = new Vector3(mPitchPoint.x, 0, mPitchPoint.z);
            mIsPithAdjustable = false;
            mIsBattingActive = true;
            mPitchT.gameObject.SetActive(false);
            mFreezedPitchGO.SetActive(true);
            mBall.useGravity = true;
            mBall.velocity = (mFreezedPitchGO.transform.position - mBall.transform.position);
            mBall.velocity = new Vector3(mBall.velocity.x * 1.5f, (mFreezedPitchGO.transform.position.y - mBall.transform.position.y), mBall.velocity.z * 1.5f);
            mBall.AddForce((mFreezedPitchGO.transform.position - mBall.transform.position).normalized * mCurrentSpeed);
            StartCoroutine("ResetAfterDelay");
        }

        /// <summary>
        /// Function to change the bowling side
        /// </summary>
        public void BowlingSideChanged ()
        {
            mBowlingSideTxt.text = (mBowlingSideToggle.isOn) ? "Right" : "Left";
            mBall.gameObject.transform.position = new Vector3 (mBall.gameObject.transform.position.x * -1, mBall.gameObject.transform.position.y, mBall.gameObject.transform.position.z);
        }

        public void ResetVairables ()
        {
            mBowlBtnGO.SetActive(true);
            StopCoroutine("ResetAfterDelay");
            mBall.transform.position = mBallInitPosition;
            mIsPithAdjustable = true;
            mIsSwingUpdated = false;
            mPitchT.gameObject.SetActive(true);
            mFreezedPitchGO.SetActive(false);
            mBall.useGravity = false;
            mBall.velocity = Vector3.zero;
        }

        /// <summary>
        /// Disable tutorial, Its Commented to mainly for everyone to family with controls (It will be shown for every launch)
        /// </summary>
        public void OnTutorialClicked ()
        {
            //PlayerPrefs.SetInt("Tutorial", 1);
            mTutorialGO.SetActive(false);
        }

        /// <summary>
        /// Function to update the swing parameter
        /// </summary>
        public void OnSwingClicked ()
        {
            float CurrentAngle = mBowlingSwinRT.transform.localEulerAngles.z;
            CurrentAngle -= 45;
            CurrentAngle = (CurrentAngle < 0) ? 180 : CurrentAngle;
            if (CurrentAngle > 90)
            {
                mSwingValue = -((CurrentAngle == 135) ? 0.25f : 0.5f);
            }
            else
            {
                mSwingValue = ((CurrentAngle == 45) ? 0.25f : 0.5f);
            }
            mSwingValue = (CurrentAngle == 90) ? 0 : mSwingValue;
            mBowlingSwinRT.rotation = Quaternion.Euler(new Vector3(0, 0, CurrentAngle));
        }

        /// <summary>
        /// Function to update the speed multiplier with base speed
        /// </summary>
        public void OnSpeedValueChanged ()
        {
            mBallSpeedMultiplier = (mBallSpeedSlider.value * 10) + 1;
            mBallSpeedMultiplier = (mBallSpeedMultiplier > 10) ? 10 : mBallSpeedMultiplier;
            mBallSpeedMultiplierTxt.text = string.Format("{0:0.0}", mBallSpeedMultiplier);
            mCurrentSpeed = mBallBaseSpeed * mBallSpeedMultiplier;
        }
    }
}
