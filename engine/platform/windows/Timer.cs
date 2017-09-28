using System.Diagnostics;

namespace RunTime.Windows
{
    public class Timer
    {
		private double _secondsPerCount;
		private double _deltaTime;
		private long _baseTime;
		private long _pausedTime;
		private long _stopTime;
		private long _prevTime;
		private long _currTime;

		private bool _isStoped;

		public float DelataTime { get { return (float)_deltaTime; } }

		public Timer()
		{
			long countsPerSec = Stopwatch.Frequency;
			_secondsPerCount = 1.0d / countsPerSec;
		}

		public float TotalTime()
		{
			if (_isStoped)
				return (float)(((_stopTime - _pausedTime) - _baseTime) * _secondsPerCount);
			else
				return (float)(((_currTime - _pausedTime) - _baseTime) * _secondsPerCount);
		}

		public void Reset()
		{
			long currTime = Stopwatch.GetTimestamp();
			_baseTime = currTime;
			_prevTime = currTime;
			_stopTime = 0;
			_isStoped = false;
		}

		public void Start()
		{
			long startTime = Stopwatch.GetTimestamp();
			if(_isStoped)
			{
				_pausedTime += (startTime - _stopTime);
				_prevTime = startTime;
				_stopTime = 0;
				_isStoped = false;
			}
		}

		public void Stop()
		{
			if(!_isStoped)
			{
				_stopTime = Stopwatch.GetTimestamp();
				_isStoped = true;
			}
		}

		public void Tick()
		{
			if(_isStoped)
			{
				_deltaTime = 0d;
				return;
			}

			long currTime = Stopwatch.GetTimestamp();
			_currTime = currTime;
			_deltaTime = (_currTime - _prevTime) * _secondsPerCount;
			_prevTime = _currTime;
			if (_deltaTime < 0d)
				_deltaTime = 0d;
		}
    }
}
