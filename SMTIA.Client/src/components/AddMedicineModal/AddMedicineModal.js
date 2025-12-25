import React, { useState, useEffect, useRef } from 'react';
import './AddMedicineModal.css';
import capsuleType1 from '../../assets/images/capsule-type-1.png';
import capsuleType2 from '../../assets/images/capsule-type-2.png';
import capsuleType3 from '../../assets/images/capsule-type-3.png';
import capsuleType4 from '../../assets/images/capsule-type-4.png';
import { smtiaApi } from '../../services/smtiaApi';

const AddMedicineModal = ({ isOpen, onClose, onNext, initialData = null }) => {
  const [medicineName, setMedicineName] = useState('');
  const [selectedType, setSelectedType] = useState('capsule');
  const [doseAmount, setDoseAmount] = useState('500');
  const [doseUnit, setDoseUnit] = useState('mg');
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [searchResults, setSearchResults] = useState([]);
  const [isSearching, setIsSearching] = useState(false);
  const scrollbarWidthRef = useRef(null);
  const searchTimerRef = useRef(null);

  const medicineTypes = [
    { id: 'capsule', label: 'Kapsül', image: capsuleType1 },
    { id: 'pill', label: 'Hap', image: capsuleType2 },
    { id: 'bottle', label: 'Şişe', image: capsuleType3 },
    { id: 'syringe', label: 'Şırınga', image: capsuleType4 }
  ];

  const doseUnits = ['mg', 'g', 'ml', 'adet'];

  const mapDosageFormToType = (dosageForm) => {
    const v = (dosageForm || '').toLowerCase();
    if (v.includes('tablet') || v.includes('pill')) return 'pill';
    if (v.includes('capsule') || v.includes('kaps')) return 'capsule';
    if (v.includes('syr') || v.includes('inject')) return 'syringe';
    if (v.includes('susp') || v.includes('bottle') || v.includes('şiş')) return 'bottle';
    return 'capsule';
  };

  const handleSelectFromSearch = (item) => {
    setMedicineName(item.name);
    setSelectedType(mapDosageFormToType(item.dosageForm));
    setSearchResults([]);
  };

  const handleNext = () => {
    if (medicineName.trim() && !isTransitioning && !isProcessing) {
      setIsProcessing(true);
      setIsTransitioning(true);
      setTimeout(() => {
        onNext({
          name: medicineName,
          type: selectedType,
          dose: {
            amount: doseAmount,
            unit: doseUnit
          }
        });
        setIsProcessing(false);
      }, 200);
    }
  };

  const handleClose = () => {
    if (isTransitioning || isProcessing) return;
    
    setIsProcessing(true);
    setIsTransitioning(true);
    setTimeout(() => {
      onClose();
      setIsProcessing(false);
    }, 200);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleClose();
    } else if (e.key === 'Enter') {
      handleNext();
    }
  };

  useEffect(() => {
    if (isOpen) {
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      scrollbarWidthRef.current = scrollbarWidth;
      
      const scrollY = window.scrollY;
      
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      document.body.style.position = 'fixed';
      document.body.style.top = `-${scrollY}px`;
      document.body.style.left = '0';
      document.body.style.right = '0';
      document.body.style.overflow = 'hidden';
      
      if (!initialData) {
        setMedicineName('');
        setSelectedType('capsule');
        setDoseAmount('500');
        setDoseUnit('mg');
        setSearchResults([]);
      } else {
        setMedicineName(initialData.name || '');
        setSelectedType(initialData.type || 'capsule');
        setDoseAmount(initialData.dose?.amount || '500');
        setDoseUnit(initialData.dose?.unit || 'mg');
        setSearchResults([]);
      }
      
      const timer = setTimeout(() => {
        const modal = document.querySelector('.add-medicine-modal');
        if (modal) {
          modal.focus();
        }
      }, 50);
      
      return () => {
        clearTimeout(timer);
        const body = document.body;
        const scrollY = body.style.top;
        body.style.position = '';
        body.style.top = '';
        body.style.left = '';
        body.style.right = '';
        body.style.paddingRight = '';
        body.style.overflow = '';
        if (scrollY) {
          window.scrollTo(0, parseInt(scrollY || '0') * -1);
        }
      };
    } else {
      const body = document.body;
      const scrollY = body.style.top;
      body.style.position = '';
      body.style.top = '';
      body.style.left = '';
      body.style.right = '';
      body.style.paddingRight = '';
      body.style.overflow = '';
      if (scrollY) {
        window.scrollTo(0, parseInt(scrollY || '0') * -1);
      }
      setIsTransitioning(false);
    }
  }, [isOpen, initialData]);

  useEffect(() => {
    if (!isOpen) return;
    const q = medicineName.trim();

    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);

    if (q.length < 2) {
      setSearchResults([]);
      return;
    }

    searchTimerRef.current = setTimeout(async () => {
      setIsSearching(true);
      try {
        const data = await smtiaApi.public.searchMedicines(q, 8);
        // unwrapResult returns { medicines, total } for SearchLocalMedicinesQueryResponse
        const items = data?.medicines || [];
        setSearchResults(items.map(x => ({
          id: x.id,
          name: x.name,
          dosageForm: x.dosageForm
        })));
      } catch {
        setSearchResults([]);
      } finally {
        setIsSearching(false);
      }
    }, 250);

    return () => {
      if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    };
  }, [medicineName, isOpen]);

  if (!isOpen) return null;

  return (
    <div 
      className={`add-medicine-modal-overlay ${isTransitioning ? 'fade-out' : 'fade-in'}`}
      onClick={handleClose}
    >
      <div 
        className={`add-medicine-modal ${isTransitioning ? 'fade-out' : 'fade-in'}`}
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        tabIndex={0}
      >

        <div className="add-medicine-header">
          <h2 className="add-medicine-title">İlaç Ekle</h2>
          <button className="add-medicine-close" onClick={handleClose}>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/>
              <line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        <div className="add-medicine-content">

          <div className="add-medicine-field">
            <label className="add-medicine-label">İlaç İsmi</label>
            <input
              type="text"
              className="add-medicine-input"
              placeholder="İlaç adını girin"
              value={medicineName}
              onChange={(e) => setMedicineName(e.target.value)}
              autoFocus
            />
            {(isSearching || searchResults.length > 0) && (
              <div style={{ marginTop: 8, border: '1px solid #e8e3d9', borderRadius: 12, overflow: 'hidden' }}>
                {isSearching && (
                  <div style={{ padding: 10, color: '#7a7a7a', fontSize: 13 }}>Aranıyor…</div>
                )}
                {!isSearching && searchResults.map(item => (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => handleSelectFromSearch(item)}
                    style={{
                      width: '100%',
                      textAlign: 'left',
                      padding: '10px 12px',
                      background: '#fff',
                      border: 'none',
                      borderTop: '1px solid #f1ede7',
                      cursor: 'pointer'
                    }}
                  >
                    {item.name}
                  </button>
                ))}
              </div>
            )}
          </div>


          <div className="add-medicine-field">
            <label className="add-medicine-label">Kapsül Tipi</label>
            <div className="add-medicine-type-options">
              {medicineTypes.map((type) => (
                <button
                  key={type.id}
                  className={`add-medicine-type-option ${selectedType === type.id ? 'selected' : ''}`}
                  onClick={() => setSelectedType(type.id)}
                >
                  <img src={type.image} alt={type.label} className="medicine-type-image" />
                </button>
              ))}
            </div>
          </div>

          <div className="add-medicine-field">
            <label className="add-medicine-label">Doz Ekle</label>
            <div className="add-medicine-dose-container">
              <input
                type="number"
                className="add-medicine-dose-input"
                value={doseAmount}
                onChange={(e) => setDoseAmount(e.target.value)}
                placeholder="500"
              />
              <select
                className="add-medicine-dose-unit"
                value={doseUnit}
                onChange={(e) => setDoseUnit(e.target.value)}
              >
                {doseUnits.map((unit) => (
                  <option key={unit} value={unit}>{unit}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        <div className="add-medicine-footer">
          <button 
            className={`add-medicine-next-button ${!medicineName.trim() ? 'disabled' : ''}`}
            onClick={handleNext}
            disabled={!medicineName.trim()}
          >
            Sonraki Sayfa
          </button>
        </div>
      </div>
    </div>
  );
};

export default AddMedicineModal;
