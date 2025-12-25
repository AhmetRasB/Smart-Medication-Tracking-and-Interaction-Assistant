import React, { useState, useEffect } from 'react';
import './LoadingPage.css';
import logo from '../../assets/logos/logo.png';
import funFacts from '../../data/ilac_funfacts.json';

const LoadingPage = ({ onLoadingComplete }) => {
  const [isVisible, setIsVisible] = useState(true);
  const [isFadingOut, setIsFadingOut] = useState(false);
  const [progress, setProgress] = useState(0);
  const [randomFact, setRandomFact] = useState('');

  useEffect(() => {
    const getRandomFact = () => {
      const randomIndex = Math.floor(Math.random() * funFacts.length);
      return funFacts[randomIndex];
    };
    
    setRandomFact(getRandomFact());

    const progressInterval = setInterval(() => {
      setProgress(prev => {
        if (prev >= 100) {
          clearInterval(progressInterval);
          return 100;
        }
    
        const increment = prev < 20 ? 1.5 : prev < 60 ? 1.2 : 0.8;
        return Math.min(prev + increment, 100);
      });
    }, 60);

    const timer = setTimeout(() => {
      setIsFadingOut(true);

      setTimeout(() => {
        setIsVisible(false);
        if (onLoadingComplete) {
          onLoadingComplete();
        }
      }, 500);
    }, 4000); 

    return () => {
      clearTimeout(timer);
      clearInterval(progressInterval);
    };
  }, [onLoadingComplete]);

  if (!isVisible) {
    return null;
  }

  return (
    <div className={`loading-container ${isFadingOut ? 'fade-out' : ''}`}>
      <div className="loading-header">
        <img src={logo} alt="Logo" className="loading-logo" />
        <span className="group-text">
          <span className="by-text">by</span> <span className="group-name">Group X</span>
        </span>
      </div>

      <div className="loading-content">
        <div className="loading-text-container">
          <p className="sample-text">{randomFact}</p>
          <div className="loading-subtitle">
            <span className="loading-dots-inline">
              <span>.</span>
              <span>.</span>
              <span>.</span>
            </span>
          </div>
        </div>
      </div>

      <div className="loading-progress-container">
        <div className="loading-progress-bar">
          <div 
            className="loading-progress-fill" 
            style={{ width: `${progress}%` }}
          >
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoadingPage;
