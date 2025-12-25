import React, { useState, useEffect } from 'react';
import './HeightSelectionPage.css';
import logo from '../../assets/logos/logo.png';
import WeightSelectionPage from '../WeightSelectionPage/WeightSelectionPage';

const HeightSelectionPage = ({ onHeightSelected, onBack, currentStep = 3, totalSteps = 6, showKVKKMessage = false, onKVKKClose, onKVKKClick, userRegistrationData = {} }) => {
  const [selectedHeight, setSelectedHeight] = useState(170);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [showBackTooltip, setShowBackTooltip] = useState(false);
  const [isScrolling, setIsScrolling] = useState(false);
  const [showWeightSelection, setShowWeightSelection] = useState(false);


  const heights = Array.from({ length: 101 }, (_, i) => i + 120);

  const handleHeightSelect = (height) => {
    if (height !== selectedHeight) {      setIsScrolling(true);
      setSelectedHeight(height);
      

      scrollToHeight(height);
 
      setTimeout(() => {
        setIsScrolling(false);
      }, 400);
    }
  };

  const scrollToHeight = (height) => {
    const container = document.querySelector('.height-selection-scroll-container');
    if (!container) return;
    
    const containerHeight = container.clientHeight;
    
    let itemHeight = 60;
    if (window.innerWidth <= 480) {
      itemHeight = 45; 
    } else if (window.innerWidth <= 768) {
      itemHeight = 50;
    }
    
    const targetScrollTop = (height - 120) * itemHeight - (containerHeight / 2) + (itemHeight / 2);
    
    container.scrollTo({
      top: Math.max(0, targetScrollTop),
      behavior: 'smooth'
    });
  };

  const handleNext = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      setShowWeightSelection(true);
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
      if (selectedHeight > 120) {
        const newHeight = selectedHeight - 1;
        handleHeightSelect(newHeight);
      }
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      if (selectedHeight < 220) {
        const newHeight = selectedHeight + 1;
        handleHeightSelect(newHeight);
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

  const handleWeightSelected = (weight) => {
    const completeData = {
      height: selectedHeight,
      weight: weight
    };
    onHeightSelected(completeData);
  };

  const handleBackFromWeightSelection = () => {
    setShowWeightSelection(false);
    setIsTransitioning(false);
  };

  const handleScroll = () => {
    setIsScrolling(true);
    clearTimeout(handleScroll.timeout);
    
    const container = document.querySelector('.height-selection-scroll-container');
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
      const heightIndex = Math.round((centerPosition - 175) / itemHeight);
      const newHeight = Math.max(120, Math.min(220, heightIndex + 120));
      
      if (newHeight !== selectedHeight) {
        setSelectedHeight(newHeight);
      }
    }
    
    handleScroll.timeout = setTimeout(() => {
      setIsScrolling(false);
    }, 150);
  };

  useEffect(() => {

    const container = document.querySelector('.height-selection-container');
    if (container) {
      container.focus();
    }
    
  
    setTimeout(() => {
      scrollToHeight(selectedHeight);
    }, 100);
  }, []);

  if (showWeightSelection) {
    return <WeightSelectionPage 
      onWeightSelected={handleWeightSelected} 
      onBack={handleBackFromWeightSelection} 
      currentStep={4} 
      totalSteps={6}
      showKVKKMessage={showKVKKMessage}
      onKVKKClose={onKVKKClose}
      onKVKKClick={onKVKKClick}
      userHeight={selectedHeight}
      userRegistrationData={{...userRegistrationData, height: selectedHeight}}
    />;
  }

  return (
    <div 
      className="height-selection-container"
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      <div className="height-selection-header">
        <div className="height-selection-logo-section">
          <img src={logo} alt="Logo" className="height-selection-logo" />
          <span className="height-selection-group-text">
            <span className="height-selection-by-text">by</span> <span className="height-selection-group-name">Group X</span>
          </span>
        </div>
        <div className="height-selection-header-right">
          <div 
            className="height-selection-back-button" 
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
          <div className="height-selection-progress-indicator">
            {currentStep}/{totalSteps}
          </div>
        </div>
      </div>

      <div className="height-selection-content">
        <div className={`height-selection-title ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <h1>Boyunuzu Seçiniz</h1>
          <p className="height-selection-subtitle">
            Boy bilginiz, ilaç dozajlarının vücut yapınıza göre ayarlanmasına yardımcı olur.
          </p>
        </div>

        <div className={`height-selection-input-container ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <div 
            className="height-selection-scroll-container"
            onScroll={handleScroll}
          >
            <div className="height-selection-center-line"></div>
            
            <div className="height-selection-numbers">
              {heights.map((height) => {
                const distance = Math.abs(height - selectedHeight);
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
                    key={height}
                    className={`height-selection-number ${sizeClass} ${isScrolling ? 'scrolling' : ''}`}
                    onClick={() => handleHeightSelect(height)}
                  >
                    {height} cm
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </div>

      <div className={`height-selection-next-page-button ${isTransitioning ? 'fade-out' : 'fade-in'}`} onClick={handleNext}>
        <span>Sonraki Sayfa</span>
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M5 12h14"/>
          <path d="M12 5l7 7-7 7"/>
        </svg>
      </div>

      {showKVKKMessage && (
        <div className="height-selection-kvkk-message">
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

export default HeightSelectionPage;
