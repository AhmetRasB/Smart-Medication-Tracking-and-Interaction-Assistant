import React, { useState, useEffect } from 'react';
import './AgeSelectionPage.css';
import logo from '../../assets/logos/logo.png';
import HeightSelectionPage from '../HeightSelectionPage/HeightSelectionPage';

const AgeSelectionPage = ({ onAgeSelected, onBack, currentStep = 2, totalSteps = 6, showKVKKMessage = false, onKVKKClose, onKVKKClick, userRegistrationData = {} }) => {
  const [selectedAge, setSelectedAge] = useState(30);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [showBackTooltip, setShowBackTooltip] = useState(false);
  const [isScrolling, setIsScrolling] = useState(false);
  const [showHeightSelection, setShowHeightSelection] = useState(false);


  const ages = Array.from({ length: 83 }, (_, i) => i + 18);

  const handleAgeSelect = (age) => {
    if (age !== selectedAge) {
  
      setIsScrolling(true);
      

      setSelectedAge(age);
      
      scrollToAge(age);
      
      setTimeout(() => {
        setIsScrolling(false);
      }, 400);
    }
  };

  const scrollToAge = (age) => {
    const container = document.querySelector('.age-selection-scroll-container');
    if (!container) return;
    
    const containerHeight = container.clientHeight;
    const agesElement = ages.findIndex(a => a === age);
    
    let itemHeight = 60;
    if (window.innerWidth <= 480) {
      itemHeight = 45; 
    } else if (window.innerWidth <= 768) {
      itemHeight = 50;
    }
    
    const targetScrollTop = (age - 18) * itemHeight - (containerHeight / 2) + (itemHeight / 2);
    
    container.scrollTo({
      top: Math.max(0, targetScrollTop),
      behavior: 'smooth'
    });
  };

  const handleNext = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      setShowHeightSelection(true);
    }, 300);
  };

  const handleBack = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      onBack();
    }, 200);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleBack();
    } else if (e.key === 'Enter') {
      handleNext();
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      if (selectedAge > 18) {
        const newAge = selectedAge - 1;
        handleAgeSelect(newAge);
      }
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      if (selectedAge < 100) {
        const newAge = selectedAge + 1;
        handleAgeSelect(newAge);
      }
    }
  };

  const handleBackMouseEnter = () => {
    const timer = setTimeout(() => {
      setShowBackTooltip(true);
    }, 2000);
    
    handleBackMouseEnter.timer = timer;
  };

  const handleBackMouseLeave = () => {
    if (handleBackMouseEnter.timer) {
      clearTimeout(handleBackMouseEnter.timer);
    }
    setShowBackTooltip(false);
  };

  const handleHeightSelected = (height) => {
    const completeData = {
      age: selectedAge,
      height: height
    };
    onAgeSelected(completeData);
  };

  const handleBackFromHeightSelection = () => {
    setShowHeightSelection(false);
    setIsTransitioning(false);
  };

  const handleScroll = () => {
    setIsScrolling(true);
    clearTimeout(handleScroll.timeout);
    
    const container = document.querySelector('.age-selection-scroll-container');
    if (container) {
      const containerHeight = container.clientHeight;
      const scrollTop = container.scrollTop;
      
      let itemHeight = 60;
      if (window.innerWidth <= 480) {
        itemHeight = 45; 
      } else if (window.innerWidth <= 768) {
        itemHeight = 50; 
      }
      
      const centerPosition = scrollTop + (containerHeight / 2);
      const ageIndex = Math.round((centerPosition - 175) / itemHeight);
      const newAge = Math.max(18, Math.min(100, ageIndex + 18));
      
      if (newAge !== selectedAge) {
        setSelectedAge(newAge);
      }
    }
    
    handleScroll.timeout = setTimeout(() => {
      setIsScrolling(false);
    }, 150);
  };

  useEffect(() => {
    const container = document.querySelector('.age-selection-container');
    if (container) {
      container.focus();
    }
    
    setTimeout(() => {
      scrollToAge(selectedAge);
    }, 100);
  }, []);

  if (showHeightSelection) {
    return <HeightSelectionPage 
      onHeightSelected={handleHeightSelected} 
      onBack={handleBackFromHeightSelection} 
      currentStep={3} 
      totalSteps={6}
      showKVKKMessage={showKVKKMessage}
      onKVKKClose={onKVKKClose}
      onKVKKClick={onKVKKClick}
      userRegistrationData={{...userRegistrationData, age: selectedAge}}
    />;
  }

  return (
    <div 
      className="age-selection-container"
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      <div className="age-selection-header">
        <div className="age-selection-logo-section">
          <img src={logo} alt="Logo" className="age-selection-logo" />
          <span className="age-selection-group-text">
            <span className="age-selection-by-text">by</span> <span className="age-selection-group-name">Group X</span>
          </span>
        </div>
        <div className="age-selection-header-right">
          <div 
            className="age-selection-back-button" 
            onClick={handleBack}
            onMouseEnter={handleBackMouseEnter}
            onMouseLeave={handleBackMouseLeave}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M19 12H5"/>
              <path d="M12 19l-7-7 7-7"/>
            </svg>
            {showBackTooltip && (
              <span className="back-tooltip">Geri dön</span>
            )}
          </div>
          <div className="age-selection-progress-indicator">
            {currentStep}/{totalSteps}
          </div>
        </div>
      </div>

      <div className="age-selection-content">
        <div className={`age-selection-title ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <h1>Yaşınızı Seçiniz</h1>
          <p className="age-selection-subtitle">
          Yaş bilginiz, ilaçların etkilerinin yaşa göre değişimini anlamamıza yardımcı olur.
          </p>
        </div>

        <div className={`age-selection-input-container ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <div 
            className="age-selection-scroll-container"
            onScroll={handleScroll}
          >
      
            <div className="age-selection-center-line"></div>
            
            <div className="age-selection-numbers">
              {ages.map((age) => {
                const distance = Math.abs(age - selectedAge);
                let sizeClass = '';
                
                if (distance === 0) {
                  sizeClass = 'selected';
                } else if (distance === 1) {
                  sizeClass = 'adjacent';
                } else if (distance === 2) {
                  sizeClass = 'near';
                } else {
                  sizeClass = 'far';
                }
                
                return (
                  <div
                    key={age}
                    className={`age-selection-number ${sizeClass} ${isScrolling ? 'scrolling' : ''}`}
                    onClick={() => handleAgeSelect(age)}
                  >
                    {age}
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </div>

      <div className={`age-selection-next-page-button ${isTransitioning ? 'fade-out' : 'fade-in'}`} onClick={handleNext}>
        <span>Sonraki Sayfa</span>
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M5 12h14"/>
          <path d="M12 5l7 7-7 7"/>
        </svg>
      </div>

      {showKVKKMessage && (
        <div className="age-selection-kvkk-message">
          <div className="kvkk-content">
            <span className="kvkk-text">Gizliliğinizde önem veriyoruz. Daha fazla bilgi için Veri ve Çerez Politikasını ziyaret edebilirsiniz.</span>
            <button className="kvkk-button" onClick={onKVKKClick}>KVKK</button>
          </div>
          <button className="kvkk-close" onClick={onKVKKClose}>
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
      )}
    </div>
  );
};

export default AgeSelectionPage;
