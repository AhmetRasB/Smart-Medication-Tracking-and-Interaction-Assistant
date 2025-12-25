import React, { useState, useEffect } from 'react';
import './AddMedicineScheduleModal.css';

const AddMedicineScheduleModal = ({ isOpen, onClose, onAddMedicine, onBack, medicineData }) => {
  const [frequency, setFrequency] = useState('daily');
  const [selectedDays, setSelectedDays] = useState([]);
  const [times, setTimes] = useState([{ time: '', dosage: '1 tablet' }]);
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [showFrequencyDropdown, setShowFrequencyDropdown] = useState(false);

  const frequencyOptions = [
    { id: 'daily', label: 'Her Gün' },
    { id: 'weekly', label: 'Gerektiğinde' },
    { id: 'custom', label: 'Özel' }
  ];

  const daysOfWeek = [
    { id: 'monday', label: 'Pzt' },
    { id: 'tuesday', label: 'Sl' },
    { id: 'wednesday', label: 'Çar' },
    { id: 'thursday', label: 'Per' },
    { id: 'friday', label: 'Cum' },
    { id: 'saturday', label: 'Cmt' },
    { id: 'sunday', label: 'Pzr' }
  ];

  const dosageOptions = ['1 tablet', '2 tablet', '1 kapsül', '2 kapsül', '5ml', '10ml'];

  useEffect(() => {
    if (frequency === 'daily') {
      setSelectedDays(daysOfWeek.map(day => day.id));
    }
  }, [frequency]);

  const handleDayToggle = (dayId) => {
    if (frequency === 'daily') {
      setFrequency('custom');
    }

    setSelectedDays(prev =>
      prev.includes(dayId)
        ? prev.filter(day => day !== dayId)
        : [...prev, dayId]
    );
  };

  const handleAddTime = () => {
    setTimes(prev => [...prev, { time: '', dosage: '1 tablet' }]);
  };

  const handleTimeChange = (index, field, value) => {
    setTimes(prev => prev.map((time, i) =>
      i === index ? { ...time, [field]: value } : time
    ));
  };

  const handleRemoveTime = (index) => {
    if (times.length > 1) {
      setTimes(prev => prev.filter((_, i) => i !== index));
    }
  };

  const handleFrequencyChange = (newFrequency) => {
    setFrequency(newFrequency);
    setShowFrequencyDropdown(false);

    if (newFrequency === 'custom') {
      setSelectedDays([]);
    }
    else if (newFrequency === 'daily') {
      setSelectedDays(daysOfWeek.map(day => day.id));
    }
    else if (newFrequency === 'weekly') {
      setSelectedDays([]);
    }
  };

  const handleBack = () => {
    if (isTransitioning || isProcessing) return;

    setIsProcessing(true);
    setIsTransitioning(true);
    setTimeout(() => {
      onBack();
      setIsProcessing(false);
    }, 200);
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


  const isFormValid = () => {
    if (!frequency) return false;

    if (frequency === 'weekly') {
      return true;
    }

    if (selectedDays.length === 0) return false;

    const validTimes = times.filter(time => time.time.trim());
    if (validTimes.length === 0) return false;

    return true;
  };

  const handleAddMedicine = () => {
    if (isTransitioning || isProcessing) return;

    if (!isFormValid()) return;

    setIsProcessing(true);
    setIsTransitioning(true);

    const scheduleData = {
      frequency,
      selectedDays: frequency === 'weekly' ? [] : selectedDays,
      times: frequency === 'weekly' ? [] : times.filter(time => time.time.trim())
    };

    const completeMedicineData = {
      ...medicineData,
      schedule: scheduleData
    };

    setTimeout(() => {
      onAddMedicine(completeMedicineData);
      setIsProcessing(false);
    }, 200);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleClose();
    } else if (e.key === 'ArrowLeft' || (e.key === 'Backspace' && e.ctrlKey)) {
      handleBack();
    }
  };

  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
      const modal = document.querySelector('.add-medicine-schedule-modal');
      if (modal) {
        modal.focus();
      }

      // Initialize from medicineData if available (Edit mode)
      if (medicineData && medicineData.schedule) {
        const sched = medicineData.schedule;
        // Determine frequency
        if (sched.selectedDays && sched.selectedDays.length === 7) {
          setFrequency('daily');
          setSelectedDays(daysOfWeek.map(day => day.id));
        } else if (!sched.selectedDays || sched.selectedDays.length === 0) {
          setFrequency('weekly'); // or custom with no days
          setSelectedDays([]);
        } else {
          setFrequency('custom');
          setSelectedDays(sched.selectedDays);
        }

        if (sched.times && sched.times.length > 0) {
          setTimes(sched.times.map(t => ({
            time: t.time,
            dosage: t.dosage || '1 tablet'
          })));
        } else {
          setTimes([{ time: '', dosage: '1 tablet' }]);
        }
      } else {
        // Default (Add mode)
        setFrequency('daily');
        setSelectedDays(daysOfWeek.map(day => day.id));
        setTimes([{ time: '', dosage: '1 tablet' }]);
      }

      setIsTransitioning(false);
      setShowFrequencyDropdown(false);
    } else {
      document.body.style.overflow = 'unset';
    }

    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen, medicineData]);


  useEffect(() => {
    const handleClickOutside = (event) => {
      if (showFrequencyDropdown && !event.target.closest('.add-medicine-schedule-frequency-container')) {
        setShowFrequencyDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showFrequencyDropdown]);

  if (!isOpen) return null;

  return (
    <div
      className={`add-medicine-schedule-modal-overlay ${isTransitioning ? 'fade-out' : 'fade-in'}`}
      onClick={handleClose}
    >
      <div
        className={`add-medicine-schedule-modal ${isTransitioning ? 'fade-out' : 'fade-in'}`}
        onClick={(e) => e.stopPropagation()}
        onKeyDown={handleKeyDown}
        tabIndex={0}
      >

        <div className="add-medicine-schedule-header">
          <div className="add-medicine-schedule-header-left">
            <button className="add-medicine-schedule-back-button" onClick={handleBack}>
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M19 12H5" />
                <path d="M12 19l-7-7 7-7" />
              </svg>
            </button>
            <h2 className="add-medicine-schedule-title">İlaç Ekle</h2>
          </div>
          <button className="add-medicine-schedule-close" onClick={handleClose}>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <line x1="18" y1="6" x2="6" y2="18" />
              <line x1="6" y1="6" x2="18" y2="18" />
            </svg>
          </button>
        </div>


        <div className="add-medicine-schedule-content">

          <div className="add-medicine-schedule-field">
            <h3 className="add-medicine-schedule-question">Bunu Ne Zaman Alıyorsun?</h3>
            <div className="add-medicine-schedule-frequency-container">
              <input
                type="text"
                className="add-medicine-schedule-frequency-input"
                value={frequencyOptions.find(f => f.id === frequency)?.label || 'Her Gün'}
                readOnly
              />
              <button
                className="add-medicine-schedule-change-button"
                onClick={() => setShowFrequencyDropdown(!showFrequencyDropdown)}
              >
                Değiştir
              </button>


              {showFrequencyDropdown && (
                <div className="add-medicine-schedule-frequency-dropdown">
                  {frequencyOptions.map((option) => (
                    <button
                      key={option.id}
                      className={`add-medicine-schedule-frequency-option ${frequency === option.id ? 'selected' : ''}`}
                      onClick={() => handleFrequencyChange(option.id)}
                    >
                      {option.label}
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>


          {frequency !== 'weekly' && (
            <div className="add-medicine-schedule-field">
              <div className="add-medicine-schedule-days-container">
                {daysOfWeek.map((day) => (
                  <button
                    key={day.id}
                    className={`add-medicine-schedule-day-button ${selectedDays.includes(day.id) ? 'selected' : ''}`}
                    onClick={() => handleDayToggle(day.id)}
                  >
                    {day.label}
                  </button>
                ))}
              </div>
            </div>
          )}


          {frequency !== 'weekly' && (
            <div className="add-medicine-schedule-field">
              <h3 className="add-medicine-schedule-question">Saat Kaçta?</h3>
              {times.map((timeData, index) => (
                <div key={index} className="add-medicine-schedule-time-container">
                  <input
                    type="time"
                    className="add-medicine-schedule-time-input"
                    value={timeData.time}
                    onChange={(e) => handleTimeChange(index, 'time', e.target.value)}
                  />
                  <select
                    className="add-medicine-schedule-dosage-select"
                    value={timeData.dosage}
                    onChange={(e) => handleTimeChange(index, 'dosage', e.target.value)}
                  >
                    {dosageOptions.map((dosage) => (
                      <option key={dosage} value={dosage}>{dosage}</option>
                    ))}
                  </select>
                  {times.length > 1 && (
                    <button
                      className="add-medicine-schedule-remove-time"
                      onClick={() => handleRemoveTime(index)}
                    >
                      <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <line x1="18" y1="6" x2="6" y2="18" />
                        <line x1="6" y1="6" x2="18" y2="18" />
                      </svg>
                    </button>
                  )}
                </div>
              ))}

              <button className="add-medicine-schedule-add-time" onClick={handleAddTime}>
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="12" y1="5" x2="12" y2="19" />
                  <line x1="5" y1="12" x2="19" y2="12" />
                </svg>
                Bir zaman ekle
              </button>
            </div>
          )}
        </div>


        <div className="add-medicine-schedule-footer">
          <button
            className={`add-medicine-schedule-add-button ${!isFormValid() ? 'disabled' : ''}`}
            onClick={handleAddMedicine}
            disabled={!isFormValid()}
          >
            İlacı Ekle
          </button>
        </div>
      </div>
    </div>
  );
};

export default AddMedicineScheduleModal;
