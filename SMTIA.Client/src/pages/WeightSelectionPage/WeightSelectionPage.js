import React, { useState, useEffect } from 'react';
import './WeightSelectionPage.css';
import logo from '../../assets/logos/logo.png';
import GenderSelectionPage from '../GenderSelectionPage/GenderSelectionPage';

const WeightSelectionPage = ({ onWeightSelected, onBack, currentStep = 4, totalSteps = 6, showKVKKMessage = false, onKVKKClose, onKVKKClick, userHeight = 170, userRegistrationData = {} }) => {
  const [selectedWeight, setSelectedWeight] = useState(70);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [showBackTooltip, setShowBackTooltip] = useState(false);
  const [isScrolling, setIsScrolling] = useState(false);
  const [showGenderSelection, setShowGenderSelection] = useState(false);

 
  const weights = Array.from({ length: 121 }, (_, i) => i + 30);

  const calculateBMI = (weight, height) => {
    const heightInMeters = height / 100;
    return weight / (heightInMeters * heightInMeters);
  };

  const getWeightStatus = (weight, height) => {
    const bmi = calculateBMI(weight, height);
    if (bmi < 18.5) return 'underweight';
    if (bmi < 25) return 'normal';
    if (bmi < 30) return 'overweight';
    return 'obese';
  };

  const getWeightStatusColors = (status) => {
    switch (status) {
      case 'underweight':
        return {
          background: '#e3f2fd', 
          border: '#2196f3',     
          message: 'Zayıf (BMI < 18.5)'
        };
      case 'normal':
        return {
          background: '#e8f5e8', 
          border: '#4caf50',     
          message: 'Normal (BMI 18.5-24.9)'
        };
      case 'overweight':
        return {
          background: '#fff3e0', 
          border: '#ff9800',     
          message: 'Fazla Kilolu (BMI 25-29.9)'
        };
      case 'obese':
        return {
          background: '#ffebee', 
          border: '#f44336',     
          message: 'Obez (BMI ≥ 30)'
        };
      default:
        return {
          background: '#f0f0f0',
          border: '#0171e4',
          message: ''
        };
    }
  };

  const handleWeightSelect = (weight) => {
    if (weight !== selectedWeight) {
      setIsScrolling(true);
      setSelectedWeight(weight);
      
      scrollToWeight(weight);
      
      setTimeout(() => {
        setIsScrolling(false);
        setSelectedWeight(weight);
      }, 600);
    }
  };

  const scrollToWeight = (weight) => {
    const container = document.querySelector('.weight-selection-scroll-container');
    if (!container) return;
    
    const selectedElement = container.querySelector(`[data-weight="${weight}"]`);
    if (selectedElement) {
      const containerWidth = container.clientWidth;
      const containerRect = container.getBoundingClientRect();
      const elementRect = selectedElement.getBoundingClientRect();
      
      const elementCenter = elementRect.left - containerRect.left + container.scrollLeft + (elementRect.width / 2);
      const targetScrollLeft = elementCenter - (containerWidth / 2);
      
      container.scrollTo({
        left: targetScrollLeft,
        behavior: 'smooth'
      });
    }
  };

  const handleNext = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      setShowGenderSelection(true);
    }, 300);
  };

  const handleBack = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      onBack();
    }, 200);
  };

  const handleGenderSelected = (gender) => {
    setIsTransitioning(true);
    setTimeout(() => {
      onWeightSelected(selectedWeight, gender);
    }, 300);
  };

  const handleBackFromGenderSelection = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      setShowGenderSelection(false);
      setIsTransitioning(false);
    }, 200);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleBack();
    } else if (e.key === 'Enter') {
      handleNext();
    } else if (e.key === 'ArrowLeft') {
      e.preventDefault();
      if (selectedWeight > 30) {
        const newWeight = selectedWeight - 1;
        handleWeightSelect(newWeight);
      }
    } else if (e.key === 'ArrowRight') {
      e.preventDefault();
      if (selectedWeight < 150) {
        const newWeight = selectedWeight + 1;
        handleWeightSelect(newWeight);
      }
    }
  };

  const handleWheelScroll = (e) => {
    const container = document.querySelector('.weight-selection-scroll-container');
    if (!container) return;
    
    if (e.deltaY !== 0) {
      e.preventDefault();
      container.scrollLeft += e.deltaY;
    } else if (e.deltaX !== 0) {
      e.preventDefault();
      container.scrollLeft += e.deltaX;
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

  const handleScroll = () => {
    setIsScrolling(true);
    clearTimeout(handleScroll.timeout);
    
    const container = document.querySelector('.weight-selection-scroll-container');
    if (container) {
      const containerWidth = container.clientWidth;
      const scrollLeft = container.scrollLeft;
      const containerCenter = scrollLeft + (containerWidth / 2);
      
      const allWeights = container.querySelectorAll('.weight-selection-number');
      let closestWeight = selectedWeight;
      let minDistance = Infinity;
      
      allWeights.forEach(element => {
        const elementRect = element.getBoundingClientRect();
        const containerRect = container.getBoundingClientRect();
        const elementCenter = elementRect.left - containerRect.left + scrollLeft + (elementRect.width / 2);
        const distance = Math.abs(elementCenter - containerCenter);
        
        if (distance < minDistance) {
          minDistance = distance;
          closestWeight = parseInt(element.getAttribute('data-weight'));
        }
      });
      
      if (closestWeight !== selectedWeight) {
        setSelectedWeight(closestWeight);
      }
    }
    
    handleScroll.timeout = setTimeout(() => {
      setIsScrolling(false);
    }, 200);
  };

  useEffect(() => {
    const container = document.querySelector('.weight-selection-container');
    if (container) {
      container.focus();
    }
    
    setTimeout(() => {
      scrollToWeight(selectedWeight);
    }, 100);
  }, []);

  if (showGenderSelection) {
    return <GenderSelectionPage 
      onGenderSelected={handleGenderSelected} 
      onBack={handleBackFromGenderSelection} 
      currentStep={5} 
      totalSteps={6}
      showKVKKMessage={showKVKKMessage}
      onKVKKClose={onKVKKClose}
      onKVKKClick={onKVKKClick}
      userRegistrationData={{...userRegistrationData, weight: selectedWeight}}
    />;
  }

  return (
    <div 
      className="weight-selection-container"
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      <div className="weight-selection-header">
        <div className="weight-selection-logo-section">
          <img src={logo} alt="Logo" className="weight-selection-logo" />
          <span className="weight-selection-group-text">
            <span className="weight-selection-by-text">by</span> <span className="weight-selection-group-name">Group X</span>
          </span>
        </div>
        <div className="weight-selection-header-right">
          <div 
            className="weight-selection-back-button" 
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
          <div className="weight-selection-progress-indicator">
            {currentStep}/{totalSteps}
          </div>
        </div>
      </div>

      <div className="weight-selection-content">
        <div className={`weight-selection-title ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <h1>Kilonuzu Seçiniz</h1>
          <p className="weight-selection-subtitle">
            Kilo bilginiz, ilaç dozajlarının vücut kitle indeksinize göre hesaplanmasına yardımcı olur.
          </p>
        </div>

        <div className={`weight-selection-input-container ${isTransitioning ? 'fade-out' : 'fade-in'}`}>
          <div 
            className="weight-selection-scroll-container"
            onScroll={handleScroll}
            onWheel={handleWheelScroll}
          >
            <div className="weight-selection-center-line"></div>
            
            <div className="weight-selection-numbers">
              {weights.map((weight) => {
                const distance = Math.abs(weight - selectedWeight);
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

                const isSelected = distance === 0;
                const weightStatus = isSelected ? getWeightStatus(weight, userHeight) : null;
                const statusColors = isSelected ? getWeightStatusColors(weightStatus) : null;
                
                const isHovered = distance === 1;
                const hoverWeightStatus = isHovered ? getWeightStatus(weight, userHeight) : null;
                const hoverColors = isHovered ? getWeightStatusColors(hoverWeightStatus) : null;
                
                return (
                  <div
                    key={weight}
                    data-weight={weight}
                    className={`weight-selection-number ${sizeClass} ${isScrolling ? 'scrolling' : ''}`}
                    onClick={() => handleWeightSelect(weight)}
                    style={isSelected && statusColors ? {
                      backgroundColor: statusColors.background,
                      borderColor: statusColors.border
                    } : {}}
                  >
                    {weight} kg
                  </div>
                );
              })}
            </div>
          </div>
          
  
          {(() => {
            const currentWeightStatus = getWeightStatus(selectedWeight, userHeight);
            const statusInfo = getWeightStatusColors(currentWeightStatus);
            return (
              <div className="weight-selection-status-info">
                <span className="status-message">{statusInfo.message}</span>
              </div>
            );
          })()}
        </div>
      </div>

      <div className={`weight-selection-next-page-button ${isTransitioning ? 'fade-out' : 'fade-in'}`} onClick={handleNext}>
        <span>Sonraki Sayfa</span>
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M5 12h14"/>
          <path d="M12 5l7 7-7 7"/>
        </svg>
      </div>

      {showKVKKMessage && (
        <div className="weight-selection-kvkk-message">
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

export default WeightSelectionPage;
