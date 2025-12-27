import React, { useState, useEffect, useRef } from 'react';
import './DashboardPage.css';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import AddMedicineModal from '../../components/AddMedicineModal/AddMedicineModal';
import AddMedicineScheduleModal from '../../components/AddMedicineScheduleModal/AddMedicineScheduleModal';
import capsuleType1 from '../../assets/images/capsule-type-1.png';
import capsuleType2 from '../../assets/images/capsule-type-2.png';
import capsuleType3 from '../../assets/images/capsule-type-3.png';
import capsuleType4 from '../../assets/images/capsule-type-4.png';
import logoAI from '../../assets/logos/logo-ai.svg';
import logo from '../../assets/logos/logo.png';
import navbarIcon1 from '../../assets/icons/navbar-1.png';
import navbarIcon2 from '../../assets/icons/navbar-2.png';
import navbarIcon3 from '../../assets/icons/navbar-3.png';
import navbarIcon4 from '../../assets/icons/navbar-4.png';
import navbarIcon5 from '../../assets/icons/navbar-5.png';
import menuIcon from '../../assets/icons/menu.png';
import notificationIcon from '../../assets/icons/natifications.png';
import profileIcon from '../../assets/icons/profile.png';
import healthyIcon from '../../assets/icons/healthy-icon.png';
import weightIcon from '../../assets/icons/weight.png';
import alarmSound from '../../assets/alarm.mp3';
import AdminPanel from '../AdminPanel/AdminPanel';
import { smtiaApi } from '../../services/smtiaApi';


delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png'
});

const DashboardPage = ({ onBack, onLogout, authUser = null, initialMedicines = [], userData = null }) => {
  const [isTransitioning, setIsTransitioning] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [showScheduleModal, setShowScheduleModal] = useState(false);
  const [currentMedicineData, setCurrentMedicineData] = useState(null);
  const [addedMedicines, setAddedMedicines] = useState([]);
  const [isLoadingMedicines, setIsLoadingMedicines] = useState(false);
  const [userInfo, setUserInfo] = useState({
    name: 'User Name',
    email: '',
    id: '',
    ageYears: null,
    heightCm: null,
    weightKg: null,
    gender: null,
    bloodType: null,
    userName: null
  });
  const [isLoadingUserInfo, setIsLoadingUserInfo] = useState(true);
  const [showFeaturePopup, setShowFeaturePopup] = useState(false);
  const [showMobileMenu, setShowMobileMenu] = useState(false);
  const [showAdminPanel, setShowAdminPanel] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [isDragging, setIsDragging] = useState(false);
  const [startX, setStartX] = useState(0);
  const [scrollLeft, setScrollLeft] = useState(0);
  const [selectedDay, setSelectedDay] = useState(new Date().getDay()); 
  const [completedMedicines, setCompletedMedicines] = useState(() => {
    
    try {
      const saved = localStorage.getItem('completedMedicines');
      return saved ? JSON.parse(saved) : {};
    } catch (error) {
      console.error('Error loading completed medicines from localStorage:', error);
      return {};
    }
  });
  const [currentDate, setCurrentDate] = useState(new Date());
  const [selectedDate, setSelectedDate] = useState(new Date()); 
  const [chatMessages, setChatMessages] = useState([]);
  const [chatInput, setChatInput] = useState('');
  const [isTyping, setIsTyping] = useState(false);
  const [hasUserMessage, setHasUserMessage] = useState(false);
  const [isMobile, setIsMobile] = useState(window.innerWidth <= 768);
  const [showAiChatPopup, setShowAiChatPopup] = useState(false);
  const [showNotifications, setShowNotifications] = useState(false);
  const [showProfileDropdown, setShowProfileDropdown] = useState(false);
  const [showSettingsModal, setShowSettingsModal] = useState(false);
  const [showWeightHeightModal, setShowWeightHeightModal] = useState(false);
  const [weightInput, setWeightInput] = useState('');
  const [heightInput, setHeightInput] = useState('');

    const isAdmin = !!authUser?.roles?.includes('Admin');
  const [isSaving, setIsSaving] = useState(false);
  const [showSideEffectModal, setShowSideEffectModal] = useState(false);
  const [selectedMedicineForSideEffect, setSelectedMedicineForSideEffect] = useState(null);
  const [selectedSeverity, setSelectedSeverity] = useState(null);
  const [selectedSideEffects, setSelectedSideEffects] = useState([]);
  const [isSavingSideEffect, setIsSavingSideEffect] = useState(false);
  const [savedSideEffects, setSavedSideEffects] = useState(() => {
    
    try {
      const saved = localStorage.getItem('savedSideEffects');
      return saved ? JSON.parse(saved) : [];
    } catch (error) {
      console.error('Error loading saved side effects from localStorage:', error);
      return [];
    }
  });
  const [showSideEffectListModal, setShowSideEffectListModal] = useState(false);
  const [showExtraHealthModal, setShowExtraHealthModal] = useState(false);
  const [showHealthCardModal, setShowHealthCardModal] = useState(false);
  const [healthData, setHealthData] = useState({
    tcKimlikNo: '',
    bloodType: '',
    smokes: false,
    cigarettesPerDay: '',
    cigarettesUnit: 'adet', 
    drinksAlcohol: false,
    hadCovid: false,
    birthCity: '',
    handedness: '', 
    surgeries: [],
    chronicDiseases: [],
    emergencyContacts: [],
    acilNot: ''
  });
  const [newSurgery, setNewSurgery] = useState('');
  const [newChronicDisease, setNewChronicDisease] = useState('');
  const [newEmergencyContact, setNewEmergencyContact] = useState({ name: '', phone: '', relationship: '' });
  const [isSavingHealthData, setIsSavingHealthData] = useState(false);
  const [medicineMessageSent, setMedicineMessageSent] = useState({}); 
  const [medicineAIDecision, setMedicineAIDecision] = useState({}); 
  const [showAlreadyConsultedPopup, setShowAlreadyConsultedPopup] = useState(false);
  const [notifications, setNotifications] = useState([
    { id: 2, message: 'Yeni ilaç eklendi: Aspirin', time: '1 saat önce', type: 'info', read: false },
  ]);
  const [isPageLoaded, setIsPageLoaded] = useState(false);
  const [isWelcomeFading, setIsWelcomeFading] = useState(false);
  const [isSensitiveDataVisible, setIsSensitiveDataVisible] = useState(false);
  const [interactionBars, setInteractionBars] = useState([]);
  const [userBodyData, setUserBodyData] = useState({
    weight: 75, 
    height: 175 
  });
  const [medicineInteractionRisk, setMedicineInteractionRisk] = useState(2);
  const previousMedicineCountRef = useRef(0);
  const riskLoadedFromBackendRef = useRef(false); 
  const [userLocation, setUserLocation] = useState(null);
  const [userAddress, setUserAddress] = useState(null);
  const [nearbyPharmacies, setNearbyPharmacies] = useState([]);
  const [isLoadingPharmacies, setIsLoadingPharmacies] = useState(false);
  const [locationPermission, setLocationPermission] = useState(null);
  const [viewMode, setViewMode] = useState('both'); 
  const [showLocationConfirm, setShowLocationConfirm] = useState(false);
  const [pendingLocation, setPendingLocation] = useState(null);
  const [selectedPharmacy, setSelectedPharmacy] = useState(null); 
  const chatMessagesRef = useRef(null);
  const chatInputRef = useRef(null);
  const [medicineTimePopup, setMedicineTimePopup] = useState(null); 
  const usedClosingMessagesRef = useRef(new Set()); 
  const bmiCardClickCountRef = useRef(0); 
  const usedBmiMessagesRef = useRef(new Set()); 
  const [shownMedicineTimes, setShownMedicineTimes] = useState(new Set()); 
  const [playedSoundTimes, setPlayedSoundTimes] = useState(new Set()); 
  const [isAlarmMuted, setIsAlarmMuted] = useState(false); 
  const isAlarmMutedRef = useRef(false); 
  const alarmIntervalRef = useRef(null); 
  const alarmAudioRef = useRef(null); 
  const originalTitleRef = useRef(document.title); 

  
  const calculateBMI = () => {
    // Use userInfo data first, fallback to userBodyData
    const weight = userInfo?.weightKg ?? userBodyData?.weight;
    const height = userInfo?.heightCm ?? userBodyData?.height;
    
    if (!weight || !height) return '--';
    const heightInMeters = height / 100;
    const bmi = weight / (heightInMeters * heightInMeters);
    return parseFloat(bmi.toFixed(1));
  };

  
  const getBMICategory = (bmi) => {
    if (bmi < 18.5) return { status: 'İdeal Kilonun Altındasın', color: '#63B3ED', position: 10 };
    if (bmi < 25) return { status: 'İdeal Kilodasın', color: '#68D391', position: 30 };
    if (bmi < 30) return { status: 'İdeal Kilonun Üzerindesin', color: '#F6AD55', position: 70 };
    return { status: 'Obezite Riski', color: '#FC8181', position: 95 };
  };

  
  const generateRandomInteractionBars = () => {
    const barCount = 13;
    const minHeight = 40;
    const maxHeight = 95;
    
    
    const whiteBarCount = Math.floor(Math.random() * 5) + 2;
    
    
    const bars = Array.from({ length: barCount }, () => ({
      height: Math.floor(Math.random() * (maxHeight - minHeight + 1)) + minHeight,
      hasWhite: false
    }));
    
    
    const whiteIndices = new Set();
    while (whiteIndices.size < whiteBarCount) {
      whiteIndices.add(Math.floor(Math.random() * barCount));
    }
    
    whiteIndices.forEach(index => {
      bars[index].hasWhite = true;
    });
    
    
    for (let i = bars.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [bars[i], bars[j]] = [bars[j], bars[i]];
    }
    
    return bars;
  };

  
  const getDaysAroundToday = () => {
    const today = new Date();
    const days = [];
    
    
    const dayRange = isMobile ? 2 : 3; 
    
    for (let i = -dayRange; i <= dayRange; i++) {
      const date = new Date(today);
      date.setDate(today.getDate() + i);
      days.push({
        date: date,
        dayIndex: date.getDay(),
        dayName: getDayName(date.getDay()),
        isToday: i === 0,
        dateString: date.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })
      });
    }
    
    return days;
  };

  const handleDaySelect = (dayInfo) => {
    setSelectedDay(dayInfo.dayIndex);
    setSelectedDate(new Date(dayInfo.date));
  };

  const handleBack = () => {
    setIsTransitioning(true);
    setTimeout(() => {
      onBack();
    }, 200);
  };

  const handleAddCapsule = () => {
    
    if (isProcessing || isTransitioning || showModal || showScheduleModal || isLoggingOut) {
      return;
    }
    
    setIsProcessing(true);
    
    
    setTimeout(() => {
      setCurrentMedicineData(null);
      setShowModal(true);
      setIsProcessing(false);
    }, 50);
  };

  const handleModalClose = () => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    setTimeout(() => {
      setShowModal(false);
      setIsTransitioning(false);
    }, 200);
  };

  const handleModalNext = (medicineData) => {
    if (isTransitioning || isProcessing) return;
    
    setCurrentMedicineData(medicineData);
    setIsTransitioning(true);
    setTimeout(() => {
      setShowModal(false);
      setIsTransitioning(false);
      setTimeout(() => {
        setShowScheduleModal(true);
      }, 100);
    }, 200);
  };

  const handleScheduleModalBack = () => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    setTimeout(() => {
      setShowScheduleModal(false);
      setIsTransitioning(false);
      setTimeout(() => {
        setShowModal(true);
      }, 100);
    }, 200);
  };

  const handleScheduleModalClose = () => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    setTimeout(() => {
      setShowScheduleModal(false);
      setIsTransitioning(false);
      setCurrentMedicineData(null);
    }, 200);
  };

  const handleAddMedicine = async (completeMedicineData) => {
    if (isTransitioning || isProcessing) return;
    
    setIsTransitioning(true);
    
    try {
      const payload = {
        name: completeMedicineData.name,
        type: completeMedicineData.type,
        dose: {
          amount: Number(completeMedicineData.dose?.amount || 1),
          unit: completeMedicineData.dose?.unit || 'adet'
        },
        schedule: {
          selectedDays: completeMedicineData.schedule?.selectedDays || [],
          times: (completeMedicineData.schedule?.times || []).map(t => ({
            time: t.time,
            dosage: t.dosage
          }))
        },
        packageSize: 0,
        note: completeMedicineData.notes || null
      };

      const created = await smtiaApi.userMedicines.add(payload);
      const newMedicine = {
        ...completeMedicineData,
        id: created.id,
        name: created.name,
        type: created.type,
        schedule: created.schedule,
        usageHistory: {}
      };

      setAddedMedicines(prev => [...prev, newMedicine]);
      setShowScheduleModal(false);
      setIsTransitioning(false);
      setCurrentMedicineData(null);
    } catch (error) {
      console.error('Add medicine API call failed:', error);
      setIsTransitioning(false);
      alert(error?.message || 'İlaç ekleme başarısız oldu.');
    }
  };

  const getMedicineIcon = (type) => {
    const icons = {
      capsule: capsuleType1,
      pill: capsuleType2,
      bottle: capsuleType3,
      syringe: capsuleType4
    };
    return icons[type] || capsuleType1;
  };

  const handleRemoveMedicine = async (index) => {
    if (isProcessing || isTransitioning) return;
    
    const medicineToRemove = addedMedicines[index];
    
    setIsProcessing(true);
    
    try {
      await smtiaApi.userMedicines.remove(medicineToRemove.id);
      setAddedMedicines(prev => prev.filter((_, i) => i !== index));
      setIsProcessing(false);
    } catch (error) {
      console.error('Remove medicine API call failed:', error);
      setIsProcessing(false);
      alert(error?.message || 'İlaç silme başarısız oldu.');
    }
  };

  
  const getDayName = (dayIndex) => {
    const days = ['P', 'S', 'Ç', 'P', 'C', 'C', 'P']; 
    return days[dayIndex];
  };

  const getDayFullName = (dayIndex) => {
    const days = ['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi', 'Pazar'];
    return days[dayIndex];
  };

  const getMedicinesForDay = (dayIndex) => {
    
    const dayMapping = {
      0: 'sunday',    
      1: 'monday',    
      2: 'tuesday',   
      3: 'wednesday', 
      4: 'thursday',  
      5: 'friday',    
      6: 'saturday'   
    };
    
    const currentDay = dayMapping[dayIndex];
    
    
    const medicinesForDay = addedMedicines.filter(medicine => {
      
      if (!medicine.schedule || !medicine.schedule.selectedDays) {
        return true;
      }
      
      
      return medicine.schedule.selectedDays.includes(currentDay);
    });
    
    
    const expandedMedicines = [];
    
    medicinesForDay.forEach((medicine, medicineIndex) => {
      const times = medicine.schedule?.times || [{ time: '08:00', dosage: '1 tablet', id: 'default' }];
      
      times.forEach((timeData, timeIndex) => {
        
        const selectedDateString = selectedDate.toISOString().split('T')[0]; 
        const usageData = medicine.usageHistory?.[selectedDateString]?.[timeData.id];
        
        expandedMedicines.push({
          ...medicine,
          id: `${medicine.id}-${timeData.id}`,
          times: [timeData], 
          dosage: timeData.dosage,
          timeIndex: timeIndex, 
          originalMedicineId: medicine.id, 
          timeId: timeData.id, 
          usageData: usageData || { taken: false, takenAt: null } 
        });
      });
    });
    
    
    return expandedMedicines.sort((a, b) => {
      const selectedDateString = selectedDate.toDateString();
      const keyA = `${a.originalMedicineId}-${a.timeIndex}-${selectedDateString}`;
      const keyB = `${b.originalMedicineId}-${b.timeIndex}-${selectedDateString}`;
      const isCompletedA = completedMedicines[keyA] || false;
      const isCompletedB = completedMedicines[keyB] || false;
      
      
      if (isCompletedA && !isCompletedB) return 1;
      if (!isCompletedA && isCompletedB) return -1;
      
      
      const timeA = a.times[0]?.time || '00:00';
      const timeB = b.times[0]?.time || '00:00';
      return timeA.localeCompare(timeB);
    });
  };

  const handleMedicineCompletion = (medicine) => {
    console.log('handleMedicineCompletion called with:', medicine);
    console.log('isProcessing:', isProcessing, 'isTransitioning:', isTransitioning);
    
    if (isProcessing || isTransitioning) {
      console.log('Blocked due to processing/transitioning state');
      return;
    }
    
    
    const key = `${medicine.originalMedicineId}-${medicine.timeIndex}-${selectedDate.toDateString()}`;
    
    
    if (completedMedicines[key] === true) {
      console.log('Medicine already completed, cannot undo');
      return;
    }
    
    const newCompletedMedicines = {
      ...completedMedicines,
      [key]: !completedMedicines[key]
    };
    
    
    setCompletedMedicines(newCompletedMedicines);
    
    
    try {
      localStorage.setItem('completedMedicines', JSON.stringify(newCompletedMedicines));
      console.log('Medicine completion saved to localStorage:', key, '=', newCompletedMedicines[key]);
    } catch (error) {
      console.error('Error saving completed medicines to localStorage:', error);
    }
    
    console.log('Medicine completion toggled locally:', newCompletedMedicines[key]);
  };

  const isMedicineCompleted = (medicine) => {
    
    const selectedDateString = selectedDate.toDateString();
    const key = `${medicine.originalMedicineId}-${medicine.timeIndex}-${selectedDateString}`;
    return completedMedicines[key] || false;
  };

  
  const isMedicineTimePassed = (medicine) => {
    if (isMedicineCompleted(medicine)) return false; 
    
    const medicineTime = medicine.times[0]?.time || '08:00';
    const [hours, minutes] = medicineTime.split(':').map(Number);
    
    const now = new Date();
    const currentHours = now.getHours();
    const currentMinutes = now.getMinutes();
    
    const currentTimeInMinutes = currentHours * 60 + currentMinutes;
    const medicineTimeInMinutes = hours * 60 + minutes;
    
    
    const today = new Date();
    const isTodaySelected = 
      selectedDate.getDate() === today.getDate() &&
      selectedDate.getMonth() === today.getMonth() &&
      selectedDate.getFullYear() === today.getFullYear();
    
    if (!isTodaySelected) return false;
    
    return currentTimeInMinutes > medicineTimeInMinutes;
  };

  
  const getHoursPassed = (medicine) => {
    const medicineTime = medicine.times[0]?.time || '08:00';
    const [hours, minutes] = medicineTime.split(':').map(Number);
    
    const now = new Date();
    const currentHours = now.getHours();
    const currentMinutes = now.getMinutes();
    
    const currentTimeInMinutes = currentHours * 60 + currentMinutes;
    const medicineTimeInMinutes = hours * 60 + minutes;
    
    const minutesPassed = currentTimeInMinutes - medicineTimeInMinutes;
    const hoursPassed = Math.floor(minutesPassed / 60);
    const remainingMinutes = minutesPassed % 60;
    
    if (hoursPassed > 0 && remainingMinutes > 0) {
      return `${hoursPassed} saat ${remainingMinutes} dakika`;
    } else if (hoursPassed > 0) {
      return `${hoursPassed} saat`;
    } else {
      return `${remainingMinutes} dakika`;
    }
  };

  
  const handleMedicineItemClick = (medicine) => {
    if (isMedicineTimePassed(medicine) && !isMedicineCompleted(medicine)) {
      
      if (medicineMessageSent[medicine.id] === 'sent') {
        setShowAlreadyConsultedPopup(true);
        setTimeout(() => {
          setShowAlreadyConsultedPopup(false);
        }, 3000); 
        return;
      }
      
      
      setMedicineMessageSent(prev => ({
        ...prev,
        [medicine.id]: 'sending'
      }));
      
      const hoursPassed = getHoursPassed(medicine);
      const medicineTime = medicine.times[0]?.time || '08:00';
      const question = `${medicine.name} ilacının saatini kaçırdım (İlaç saati: ${medicineTime}, ${hoursPassed} geçti). Şimdi içebilir miyim?`;
      
      
      if (!hasUserMessage) {
        setIsWelcomeFading(true);
      }
      
      const userMessage = {
        id: Date.now(),
        type: 'user',
        text: question,
        timestamp: new Date()
      };
      
      setChatMessages(prev => [...prev, userMessage]);
      setChatInput('');
      setIsTyping(true);
      
      
      setTimeout(() => {
        setHasUserMessage(true);
        
        setMedicineMessageSent(prev => ({
          ...prev,
          [medicine.id]: 'sent'
        }));
      }, 300);
      
      
      const sendChatMessage = async () => {
        try {
          await new Promise(resolve => setTimeout(resolve, 400));
          
          const apiResponse = await smtiaApi.chat.send(question);
          const apiAiReply = apiResponse?.reply || 'Bunu doktoruna danışmanı öneririm.';
          
          const hoursPassed = getHoursPassed(medicine);
          const hoursPassedNum = parseInt(hoursPassed.split(' ')[0]) || 0;
          
          
          const canTake = hoursPassedNum < 2;
          
          const aiResponseText = canTake
            ? `${medicine.name} ilacınızı şu anda alabilirsiniz. ${hoursPassed} geçmiş olmasına rağmen, bir sonraki doz zamanına kadar yeterli süre var. (${apiAiReply})`
            : `${medicine.name} ilacınız için önerim, bir sonraki doz zamanını beklemek. ${hoursPassed} geçtiği için şu anda almanız önerilmez. Lütfen doktorunuza danışın veya ilaç prospektüsünü kontrol edin. (${apiAiReply})`;
          
          const aiMessage = {
            id: Date.now() + 1,
            type: 'ai',
            text: aiResponseText,
            timestamp: new Date()
          };
          
          setChatMessages(prev => [...prev, aiMessage]);
          setIsTyping(false);
          
          
          setMedicineAIDecision(prev => ({
            ...prev,
            [medicine.id]: canTake
          }));
        } catch (error) {
          console.error('Chat API error:', error);
          
          
          const hoursPassed = getHoursPassed(medicine);
          const hoursPassedNum = parseInt(hoursPassed.split(' ')[0]) || 0;
          const canTake = hoursPassedNum < 2;
          
          const aiResponseText = canTake 
            ? `${medicine.name} ilacınızı şu anda alabilirsiniz. ${hoursPassed} geçmiş olmasına rağmen, bir sonraki doz zamanına kadar yeterli süre var.`
            : `${medicine.name} ilacınız için önerim, bir sonraki doz zamanını beklemek. ${hoursPassed} geçtiği için şu anda almanız önerilmez. Lütfen doktorunuza danışın veya ilaç prospektüsünü kontrol edin.`;
          
          const aiResponse = {
            id: Date.now() + 1,
            type: 'ai',
            text: aiResponseText,
            timestamp: new Date()
          };
          setChatMessages(prev => [...prev, aiResponse]);
          setIsTyping(false);
          
          
          setMedicineAIDecision(prev => ({
            ...prev,
            [medicine.id]: canTake
          }));
        }
      };
      
      sendChatMessage();
    }
  };

  
  const clearCompletedMedicines = () => {
    localStorage.removeItem('completedMedicines');
    setCompletedMedicines({});
    console.log('Completed medicines cleared from localStorage');
  };

  
  const showCompletedMedicines = () => {
    console.log('Current completed medicines:', completedMedicines);
    console.log('localStorage completed medicines:', localStorage.getItem('completedMedicines'));
  };

  const handleFeatureClick = () => {
    setShowFeaturePopup(true);
  };

  const handleNotificationClick = () => {
    setShowNotifications(!showNotifications);
  };

  const handleNotificationClose = () => {
    setShowNotifications(false);
  };

  const handleProfileClick = () => {
    setShowProfileDropdown(!showProfileDropdown);
  };

  const handleProfileClose = () => {
    setShowProfileDropdown(false);
  };

  const handleSettingsClick = () => {
    setShowSettingsModal(true);
    setShowProfileDropdown(false);
  };

  const handleSettingsClose = () => {
    setShowSettingsModal(false);
  };

  const handleWeightHeightClick = () => {
    setWeightInput(userBodyData.weight.toString());
    setHeightInput(userBodyData.height.toString());
    setShowWeightHeightModal(true);
    setShowSettingsModal(false);
  };

  const handleWeightHeightClose = () => {
    setShowWeightHeightModal(false);
    setWeightInput('');
    setHeightInput('');
  };

  const handleWeightHeightSave = async () => {
    const newWeight = parseFloat(weightInput);
    const newHeight = parseFloat(heightInput);
    
    if (isNaN(newWeight) || isNaN(newHeight) || newWeight <= 0 || newHeight <= 0) {
      alert('Lütfen geçerli kilo ve boy değerleri girin.');
      return;
    }

    setIsSaving(true);
    
    try {
      // Call API to update weight and height
      await smtiaApi.profile.update({
        weightKg: newWeight,
        heightCm: newHeight
      });
      
      // Update local state
      setUserBodyData({
        weight: newWeight,
        height: newHeight
      });
      
      // Also update userInfo to keep it in sync
      setUserInfo(prev => ({
        ...prev,
        weightKg: newWeight,
        heightCm: newHeight
      }));
      
      alert('Kilo ve boy bilgileriniz başarıyla güncellendi.');
      handleWeightHeightClose();
    } catch (error) {
      console.error('Error saving weight/height:', error);
      const errorMessage = error?.message || 'Kilo ve boy bilgileri kaydedilirken bir hata oluştu.';
      alert(errorMessage);
    } finally {
      setIsSaving(false);
    }
  };

  const calculateWeightChange = () => {
    const oldWeight = userBodyData.weight;
    const newWeight = parseFloat(weightInput);
    if (isNaN(newWeight) || newWeight === oldWeight) return null;
    const diff = newWeight - oldWeight;
    return diff > 0 ? `+${diff.toFixed(1)} kg kilo almışsın` : `${diff.toFixed(1)} kg kilo vermişsin`;
  };

  const calculateHeightChange = () => {
    const oldHeight = userBodyData.height;
    const newHeight = parseFloat(heightInput);
    if (isNaN(newHeight) || newHeight === oldHeight) return null;
    const diff = newHeight - oldHeight;
    return diff > 0 ? `+${diff.toFixed(0)} cm artmışsın` : `${diff.toFixed(0)} cm azalmışsın`;
  };

  const handleSideEffectClick = () => {
    setShowSideEffectModal(true);
    setShowSettingsModal(false);
    setSelectedMedicineForSideEffect(null);
    setSelectedSeverity(null);
    setSelectedSideEffects([]);
  };

  const handleSideEffectClose = () => {
    setShowSideEffectModal(false);
    setSelectedMedicineForSideEffect(null);
    setSelectedSeverity(null);
    setSelectedSideEffects([]);
  };

  const handleSideEffectSave = async () => {
    if (!selectedMedicineForSideEffect || !selectedSeverity || selectedSideEffects.length === 0) {
      return;
    }

    setIsSavingSideEffect(true);
    
    try {
      // Prepare payload for API
      const payload = {
        medicineId: selectedMedicineForSideEffect.id || null,
        medicineName: selectedMedicineForSideEffect.name || 'Bilinmeyen İlaç',
        severity: selectedSeverity,
        sideEffects: selectedSideEffects, // Array of side effect strings
        date: new Date().toISOString()
      };

      // Save to backend
      const response = await smtiaApi.sideEffects.add(payload);
      
      // Also update local state for immediate UI update
      const newSideEffect = {
        id: response.id || Date.now(),
        medicine: selectedMedicineForSideEffect,
        severity: selectedSeverity,
        sideEffects: selectedSideEffects,
        date: new Date().toISOString()
      };
      
      const updatedSideEffects = [...savedSideEffects, newSideEffect];
      setSavedSideEffects(updatedSideEffects);
      localStorage.setItem('savedSideEffects', JSON.stringify(updatedSideEffects));
      
      console.log('Side effect saved to backend:', response);
      
      alert('Yan etki başarıyla kaydedildi.');
      handleSideEffectClose();
    } catch (error) {
      console.error('Error saving side effect:', error);
      alert(error?.message || 'Yan etki kaydedilirken bir hata oluştu.');
    } finally {
      setIsSavingSideEffect(false);
    }
  };

  
  const handleSideEffectCardClick = () => {
    
    if (savedSideEffects.length === 0) {
      return;
    }
    
    setShowHealthCardModal(false);
    
    window.scrollTo({ top: 0, behavior: 'instant' });
    setShowSideEffectListModal(true);
  };

  const handleSideEffectListModalClose = () => {
    setShowSideEffectListModal(false);
  };

  const toggleSideEffect = (sideEffect) => {
    setSelectedSideEffects(prev => 
      prev.includes(sideEffect)
        ? prev.filter(e => e !== sideEffect)
        : [...prev, sideEffect]
    );
  };

  const sideEffectOptions = [
    'Anafilaktik Şok',
    'Kızarıklık',
    'Kaşıntı',
    'Nefes Darlığı',
    'Mide Bulantısı',
    'Kusma',
    'Baş Dönmesi',
    'Baş Ağrısı',
    'Uyku Hali',
    'Halsizlik',
    'Ateş',
    'Titreme',
    'İshal',
    'Kabızlık',
    'Karın Ağrısı',
    'Göz Kızarıklığı',
    'Burun Akıntısı',
    'Öksürük',
    'Cilt Döküntüsü',
    'Şişlik'
  ];

  const severityOptions = [
    { value: 'mild', label: 'Hafif' },
    { value: 'moderate', label: 'Orta' },
    { value: 'severe', label: 'Şiddetli' },
    { value: 'critical', label: 'Kritik' }
  ];

  const handleExtraHealthClick = async () => {
    setShowSettingsModal(false);
    
    // Load latest health data when opening modal
    try {
      const health = await smtiaApi.profile.getHealth();
      setHealthData({
        tcKimlikNo: health.tcIdentityNo || '',
        bloodType: health.bloodType || '',
        smokes: health.smokes ?? false,
        cigarettesPerDay: health.cigarettesPerDay?.toString() || '',
        cigarettesUnit: health.cigarettesUnit || 'adet',
        drinksAlcohol: health.drinksAlcohol ?? false,
        hadCovid: health.hadCovid ?? false,
        birthCity: health.birthCity || '',
        handedness: health.handedness || '',
        surgeries: health.surgeries || [],
        chronicDiseases: health.chronicDiseases || [],
        emergencyContacts: health.emergencyContacts || [],
        acilNot: health.acilNot || ''
      });
    } catch (error) {
      console.error('Error loading health data:', error);
      // Keep existing healthData if load fails
    }
    
    setShowExtraHealthModal(true);
  };

  const handleExtraHealthClose = () => {
    setShowExtraHealthModal(false);
  };

  
  const handleHealthCardClick = async () => {
    setShowSideEffectListModal(false);
    
    // Load latest health data and user info when opening health card modal
    try {
      const [health, userProfile] = await Promise.all([
        smtiaApi.profile.getHealth().catch(e => {
          console.warn('Could not fetch health data', e);
          return {};
        }),
        smtiaApi.profile.me().catch(e => {
          console.warn('Could not fetch user profile', e);
          return null;
        })
      ]);
      
      // Update health data
      setHealthData({
        tcKimlikNo: health.tcIdentityNo || '',
        bloodType: health.bloodType || '',
        smokes: health.smokes ?? false,
        cigarettesPerDay: health.cigarettesPerDay?.toString() || '',
        cigarettesUnit: health.cigarettesUnit || 'adet',
        drinksAlcohol: health.drinksAlcohol ?? false,
        hadCovid: health.hadCovid ?? false,
        birthCity: health.birthCity || '',
        handedness: health.handedness || '',
        surgeries: health.surgeries || [],
        chronicDiseases: health.chronicDiseases || [],
        emergencyContacts: health.emergencyContacts || [],
        acilNot: health.acilNot || ''
      });
      
      // Update user info and body data if profile was loaded
      if (userProfile) {
        setUserInfo(prev => ({
          ...prev,
          weightKg: userProfile.weightKg ?? prev.weightKg,
          heightCm: userProfile.heightCm ?? prev.heightCm,
          name: userProfile.name || prev.name
        }));
        
        if (userProfile.weightKg != null && userProfile.heightCm != null) {
          setUserBodyData({
            weight: userProfile.weightKg,
            height: userProfile.heightCm
          });
        }
      }
    } catch (error) {
      console.error('Error loading data for health card:', error);
      // Keep existing data if load fails
    }
    
    setShowHealthCardModal(true);
  };

  const handleHealthCardModalClose = () => {
    setShowHealthCardModal(false);
  };

  
  const handleExportHealthCardPDF = async () => {
    try {
      const healthCardElement = document.querySelector('.health-card-id');
      if (!healthCardElement) {
        console.error('Health card element not found');
        return;
      }

      
      const originalDisplay = healthCardElement.style.display;
      healthCardElement.style.display = 'block';

      
      const canvas = await html2canvas(healthCardElement, {
        scale: 2,
        useCORS: true,
        logging: false,
        backgroundColor: '#ffffff',
        width: healthCardElement.scrollWidth,
        height: healthCardElement.scrollHeight,
      });

      const imgData = canvas.toDataURL('image/png');
      
      
      const pdf = new jsPDF({
        orientation: 'portrait',
        unit: 'mm',
        format: 'a4'
      });

      const imgWidth = 210; 
      const pageHeight = 297; 
      const imgHeight = (canvas.height * imgWidth) / canvas.width;
      let heightLeft = imgHeight;
      let position = 0;

      
      pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
      heightLeft -= pageHeight;

      
      while (heightLeft >= 0) {
        position = heightLeft - imgHeight;
        pdf.addPage();
        pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
        heightLeft -= pageHeight;
      }

      
      const fileName = `Saglik_Karti_${userInfo?.name || 'Kullanici'}_${new Date().toISOString().split('T')[0]}.pdf`;
      pdf.save(fileName);

      
      healthCardElement.style.display = originalDisplay;
    } catch (error) {
      console.error('Error generating PDF:', error);
      alert('PDF oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.');
    }
  };

  const handleExtraHealthSave = async () => {
    setIsSavingHealthData(true);
    try {
      // Ensure arrays are always arrays (not null/undefined)
      const surgeries = Array.isArray(healthData.surgeries) ? healthData.surgeries : [];
      const chronicDiseases = Array.isArray(healthData.chronicDiseases) ? healthData.chronicDiseases : [];
      const emergencyContacts = Array.isArray(healthData.emergencyContacts) ? healthData.emergencyContacts : [];
      
      const payload = {
        tcIdentityNo: healthData.tcKimlikNo || null,
        bloodType: healthData.bloodType || null,
        smokes: healthData.smokes ?? null,
        cigarettesPerDay: healthData.cigarettesPerDay ? parseInt(healthData.cigarettesPerDay) : null,
        cigarettesUnit: healthData.cigarettesUnit || null,
        drinksAlcohol: healthData.drinksAlcohol ?? null,
        hadCovid: healthData.hadCovid ?? null,
        birthCity: healthData.birthCity || null,
        handedness: healthData.handedness || null,
        acilNot: healthData.acilNot || null,
        emergencyContacts: emergencyContacts.length > 0 ? emergencyContacts.map(c => ({
          name: c.name || '',
          phone: c.phone || '',
          relationship: c.relationship || null
        })) : null,
        surgeries: surgeries.length > 0 ? surgeries.filter(s => s && s.trim()) : null,
        chronicDiseases: chronicDiseases.length > 0 ? chronicDiseases.filter(d => d && d.trim()) : null
      };

      console.log('Saving health data payload:', payload);

      await smtiaApi.profile.updateHealth(payload);

      alert('Sağlık bilgileriniz başarıyla kaydedildi.');
      handleExtraHealthClose();
    } catch (error) {
      console.error('Error saving health data:', error);
      
      // Handle concurrency conflict - reload data and show message
      if (error?.status === 409 || error?.message?.includes('modified by another process')) {
        try {
          // Reload health data from server
          const health = await smtiaApi.profile.getHealth();
          setHealthData({
            tcKimlikNo: health.tcIdentityNo || '',
            bloodType: health.bloodType || '',
            smokes: health.smokes ?? false,
            cigarettesPerDay: health.cigarettesPerDay?.toString() || '',
            cigarettesUnit: health.cigarettesUnit || 'adet',
            drinksAlcohol: health.drinksAlcohol ?? false,
            hadCovid: health.hadCovid ?? false,
            birthCity: health.birthCity || '',
            handedness: health.handedness || '',
            surgeries: health.surgeries || [],
            chronicDiseases: health.chronicDiseases || [],
            emergencyContacts: health.emergencyContacts || [],
            acilNot: health.acilNot || ''
          });
          alert('Veriler başka bir işlem tarafından değiştirilmiş. Lütfen tekrar deneyin. Veriler yenilendi.');
        } catch (reloadError) {
          console.error('Error reloading health data:', reloadError);
          alert('Veriler güncellenemedi. Lütfen sayfayı yenileyip tekrar deneyin.');
        }
      } else {
        const errorMessage = error?.message || 'Sağlık bilgileri kaydedilirken bir hata oluştu.';
        alert(errorMessage);
      }
    } finally {
      setIsSavingHealthData(false);
    }
  };

  const addSurgery = () => {
    if (newSurgery.trim()) {
      setHealthData(prev => ({
        ...prev,
        surgeries: [...prev.surgeries, newSurgery.trim()]
      }));
      setNewSurgery('');
    }
  };

  const removeSurgery = (index) => {
    setHealthData(prev => ({
      ...prev,
      surgeries: prev.surgeries.filter((_, i) => i !== index)
    }));
  };

  const addChronicDisease = () => {
    if (newChronicDisease.trim()) {
      setHealthData(prev => ({
        ...prev,
        chronicDiseases: [...prev.chronicDiseases, newChronicDisease.trim()]
      }));
      setNewChronicDisease('');
    }
  };

  const removeChronicDisease = (index) => {
    setHealthData(prev => ({
      ...prev,
      chronicDiseases: prev.chronicDiseases.filter((_, i) => i !== index)
    }));
  };

  const addEmergencyContact = () => {
    if (newEmergencyContact.name.trim() && newEmergencyContact.phone.trim() && newEmergencyContact.relationship.trim()) {
      setHealthData(prev => ({
        ...prev,
        emergencyContacts: [...prev.emergencyContacts, { ...newEmergencyContact }]
      }));
      setNewEmergencyContact({ name: '', phone: '', relationship: '' });
    }
  };

  const removeEmergencyContact = (index) => {
    setHealthData(prev => ({
      ...prev,
      emergencyContacts: prev.emergencyContacts.filter((_, i) => i !== index)
    }));
  };

  const bloodTypes = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', '0+', '0-'];
  const turkishCities = [
    'Adana', 'Adıyaman', 'Afyonkarahisar', 'Ağrı', 'Amasya', 'Ankara', 'Antalya', 'Artvin',
    'Aydın', 'Balıkesir', 'Bilecik', 'Bingöl', 'Bitlis', 'Bolu', 'Burdur', 'Bursa',
    'Çanakkale', 'Çankırı', 'Çorum', 'Denizli', 'Diyarbakır', 'Edirne', 'Elazığ', 'Erzincan',
    'Erzurum', 'Eskişehir', 'Gaziantep', 'Giresun', 'Gümüşhane', 'Hakkari', 'Hatay', 'Isparta',
    'İçel (Mersin)', 'İstanbul', 'İzmir', 'Kars', 'Kastamonu', 'Kayseri', 'Kırklareli', 'Kırşehir',
    'Kocaeli', 'Konya', 'Kütahya', 'Malatya', 'Manisa', 'Kahramanmaraş', 'Mardin', 'Muğla',
    'Muş', 'Nevşehir', 'Niğde', 'Ordu', 'Rize', 'Sakarya', 'Samsun', 'Siirt',
    'Sinop', 'Sivas', 'Tekirdağ', 'Tokat', 'Trabzon', 'Tunceli', 'Şanlıurfa', 'Uşak',
    'Van', 'Yozgat', 'Zonguldak', 'Aksaray', 'Bayburt', 'Karaman', 'Kırıkkale', 'Batman',
    'Şırnak', 'Bartın', 'Ardahan', 'Iğdır', 'Yalova', 'Karabük', 'Kilis', 'Osmaniye', 'Düzce'
  ];

  const markNotificationAsRead = (id) => {
    setNotifications(notifications.map(notif => 
      notif.id === id ? { ...notif, read: true } : notif
    ));
  };

  const deleteNotification = (id, e) => {
    e.stopPropagation(); 
    setNotifications(notifications.filter(notif => notif.id !== id));
  };

  
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (showNotifications && !event.target.closest('.notification-button-wrapper')) {
        setShowNotifications(false);
      }
    };

    if (showNotifications) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showNotifications]);

  
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (showProfileDropdown && !event.target.closest('.profile-button-wrapper')) {
        setShowProfileDropdown(false);
      }
    };

    if (showProfileDropdown) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showProfileDropdown]);

  
  useEffect(() => {
    if (showSettingsModal) {
      
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      
      
      const scrollY = window.scrollY;
      
      
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      document.body.style.position = 'fixed';
      document.body.style.top = `-${scrollY}px`;
      document.body.style.left = '0';
      document.body.style.right = '0';
      document.body.style.overflow = 'hidden';
      
      return () => {
        
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
    }
  }, [showSettingsModal]);

  
  useEffect(() => {
    if (showWeightHeightModal) {
      
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      
      
      const scrollY = window.scrollY;
      
      
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      document.body.style.position = 'fixed';
      document.body.style.top = `-${scrollY}px`;
      document.body.style.left = '0';
      document.body.style.right = '0';
      document.body.style.overflow = 'hidden';
      
      return () => {
        
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
    }
  }, [showWeightHeightModal]);

  
  useEffect(() => {
    if (showSideEffectListModal) {
      
      window.scrollTo({ top: 0, behavior: 'instant' });
      
      
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      
      
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      document.body.style.position = 'fixed';
      document.body.style.top = '0';
      document.body.style.left = '0';
      document.body.style.right = '0';
      document.body.style.overflow = 'hidden';
      
      return () => {
        
        const body = document.body;
        body.style.position = '';
        body.style.top = '';
        body.style.left = '';
        body.style.right = '';
        body.style.paddingRight = '';
        body.style.overflow = '';
      };
    }
  }, [showSideEffectListModal]);

  
  useEffect(() => {
    if (showSideEffectModal) {
      
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      
      
      const scrollY = window.scrollY;
      
      
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      document.body.style.position = 'fixed';
      document.body.style.top = `-${scrollY}px`;
      document.body.style.left = '0';
      document.body.style.right = '0';
      document.body.style.overflow = 'hidden';
      
      return () => {
        
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
    }
  }, [showSideEffectModal]);

  
  useEffect(() => {
    if (showHealthCardModal) {
      
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      
      
      const scrollY = window.scrollY;
      
      
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      document.body.style.position = 'fixed';
      document.body.style.top = `-${scrollY}px`;
      document.body.style.left = '0';
      document.body.style.right = '0';
      document.body.style.overflow = 'hidden';
      
      return () => {
        
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
    }
  }, [showHealthCardModal]);

  
  useEffect(() => {
    if (showExtraHealthModal) {
      
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      
      
      const scrollY = window.scrollY;
      
      
      if (scrollbarWidth > 0) {
        document.body.style.paddingRight = `${scrollbarWidth}px`;
      }
      document.body.style.position = 'fixed';
      document.body.style.top = `-${scrollY}px`;
      document.body.style.left = '0';
      document.body.style.right = '0';
      document.body.style.overflow = 'hidden';
      
      return () => {
        
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
    }
  }, [showExtraHealthModal]);

  
  const playAlarmSound = () => {
    
    if (isAlarmMutedRef.current) {
      return;
    }
    
    try {
      
      if (!alarmAudioRef.current) {
        console.log('Alarm sesi oluşturuluyor...', alarmSound);
        alarmAudioRef.current = new Audio(alarmSound);
        alarmAudioRef.current.volume = 0.8; 
        alarmAudioRef.current.loop = false; 
        alarmAudioRef.current.preload = 'auto';
        
        
        alarmAudioRef.current.addEventListener('error', (e) => {
          console.error('Alarm sesi yüklenemedi:', e, alarmAudioRef.current.error);
        });
        
        
        alarmAudioRef.current.addEventListener('loadeddata', () => {
          console.log('Alarm sesi yüklendi, süre:', alarmAudioRef.current.duration);
        });
        
        
        alarmAudioRef.current.load();
      }
      
      
      if (alarmAudioRef.current.readyState >= 2) {
        
        alarmAudioRef.current.currentTime = 0;
        const playPromise = alarmAudioRef.current.play();
        
        if (playPromise !== undefined) {
          playPromise
            .then(() => {
              console.log('Alarm sesi çalıyor');
            })
            .catch(error => {
              console.error('Alarm sesi çalınamadı:', error);
              
              
            });
        }
      } else {
        
        console.log('Alarm sesi yükleniyor, bekleniyor...');
        alarmAudioRef.current.addEventListener('canplay', () => {
          alarmAudioRef.current.currentTime = 0;
          alarmAudioRef.current.play().catch(error => {
            console.error('Alarm sesi çalınamadı:', error);
          });
        }, { once: true });
      }
    } catch (error) {
      console.error('Alarm sesi oluşturulamadı:', error);
    }
  };

  
  const startAlarmSound = () => {
    
    if (alarmIntervalRef.current) {
      clearInterval(alarmIntervalRef.current);
    }
    
    
    if (!alarmAudioRef.current) {
      alarmAudioRef.current = new Audio(alarmSound);
      alarmAudioRef.current.volume = 0.8;
      alarmAudioRef.current.loop = false;
      alarmAudioRef.current.preload = 'auto';
      
      
      const handleCanPlay = () => {
        console.log('Alarm sesi hazır, çalınıyor');
        playAlarmSound();
        
        
        alarmIntervalRef.current = setInterval(() => {
          
          if (!isAlarmMutedRef.current) {
            playAlarmSound();
          }
        }, 28000); 
      };
      
      alarmAudioRef.current.addEventListener('canplaythrough', handleCanPlay, { once: true });
      
      
      alarmAudioRef.current.addEventListener('error', (e) => {
        console.error('Alarm sesi yüklenemedi:', e);
        
        alarmIntervalRef.current = setInterval(() => {
          
          if (!isAlarmMutedRef.current) {
            playAlarmSound();
          }
        }, 28000); 
      });
      
      
      alarmAudioRef.current.load();
    } else {
      
      playAlarmSound();
      
      
      alarmIntervalRef.current = setInterval(() => {
        
        if (!isAlarmMutedRef.current) {
          playAlarmSound();
        }
      }, 28000); 
    }
  };

  
  const stopAlarmSound = () => {
    if (alarmIntervalRef.current) {
      clearInterval(alarmIntervalRef.current);
      alarmIntervalRef.current = null;
    }
    
    
    if (alarmAudioRef.current) {
      alarmAudioRef.current.pause();
      alarmAudioRef.current.currentTime = 0;
    }
  };

  
  const toggleAlarmMute = () => {
    setIsAlarmMuted(prev => {
      const newMuted = !prev;
      
      isAlarmMutedRef.current = newMuted;
      
      if (newMuted && alarmAudioRef.current) {
        alarmAudioRef.current.pause();
        alarmAudioRef.current.currentTime = 0;
      }
      return newMuted;
    });
  };

  
  useEffect(() => {
    if (!alarmAudioRef.current) {
      console.log('Alarm sesi önceden yükleniyor...');
      alarmAudioRef.current = new Audio(alarmSound);
      alarmAudioRef.current.volume = 0.8;
      alarmAudioRef.current.loop = false;
      alarmAudioRef.current.preload = 'auto';
      
      alarmAudioRef.current.addEventListener('loadeddata', () => {
        console.log('Alarm sesi önceden yüklendi, süre:', alarmAudioRef.current.duration);
      });
      
      alarmAudioRef.current.addEventListener('error', (e) => {
        console.error('Alarm sesi yüklenemedi:', e, alarmAudioRef.current.error);
      });
      
      
      alarmAudioRef.current.load();
    }
  }, []);

  
  useEffect(() => {
    const checkMedicineTimes = () => {
      const now = new Date();
      const currentHours = now.getHours();
      const currentMinutes = now.getMinutes();
      const currentSeconds = now.getSeconds();
      const currentTimeInMinutes = currentHours * 60 + currentMinutes;
      
      
      const todayMedicines = getMedicinesForDay(new Date().getDay());
      
      todayMedicines.forEach((medicine) => {
        
        if (isMedicineCompleted(medicine)) return;
        
        const medicineTime = medicine.times[0]?.time || '08:00';
        const [hours, minutes] = medicineTime.split(':').map(Number);
        const medicineTimeInMinutes = hours * 60 + minutes;
        
        
        const timeDiff = currentTimeInMinutes - medicineTimeInMinutes;
        const uniqueKey = `${medicine.id}-${medicineTime}`;
        const soundKey = `${medicine.id}-${medicineTime}-${currentHours}-${currentMinutes}`;
        
        
        if (timeDiff >= 0 && timeDiff <= 1 && !shownMedicineTimes.has(uniqueKey)) {
          
          setMedicineTimePopup({
            medicine: medicine,
            time: medicineTime
          });
          
          setShownMedicineTimes(prev => new Set([...prev, uniqueKey]));
          
          startAlarmSound();
        }
      });
    };

    
    checkMedicineTimes();
    
    
    const interval = setInterval(checkMedicineTimes, 5000);
    
    return () => {
      clearInterval(interval);
      stopAlarmSound(); 
    };
    
  }, [addedMedicines, completedMedicines]);

  
  useEffect(() => {
    if (medicineTimePopup) {
      
      if (!originalTitleRef.current || originalTitleRef.current === 'İlaç Saati Geldi !') {
        
        const savedTitle = localStorage.getItem('originalPageTitle');
        if (savedTitle && savedTitle !== 'İlaç Saati Geldi !') {
          originalTitleRef.current = savedTitle;
        } else {
          
          if (document.title !== 'İlaç Saati Geldi !') {
            originalTitleRef.current = document.title;
            localStorage.setItem('originalPageTitle', document.title);
          }
        }
      }
      
      document.title = 'İlaç Saati Geldi !';
    } else {
      
      if (originalTitleRef.current && originalTitleRef.current !== 'İlaç Saati Geldi !') {
        document.title = originalTitleRef.current;
      }
    }
  }, [medicineTimePopup]);

  
  useEffect(() => {
    if (medicineTimePopup && medicineTimePopup.medicine) {
      const medicine = medicineTimePopup.medicine;
      if (isMedicineCompleted(medicine)) {
        stopAlarmSound();
        
        document.title = originalTitleRef.current;
        setMedicineTimePopup(null);
      }
    }
    
  }, [completedMedicines, medicineTimePopup]);

  const handleFeaturePopupClose = () => {
    setShowFeaturePopup(false);
  };

  
  const handleMedicineTimePopupClose = () => {
    stopAlarmSound(); 
    setIsAlarmMuted(false); 
    isAlarmMutedRef.current = false; 
    
    document.title = originalTitleRef.current;
    setMedicineTimePopup(null);
  };

  
  const handleMedicineTaken = () => {
    if (medicineTimePopup && medicineTimePopup.medicine) {
      const medicine = medicineTimePopup.medicine;
      stopAlarmSound(); 
      setIsAlarmMuted(false); 
      isAlarmMutedRef.current = false; 
      
      document.title = originalTitleRef.current;
      handleMedicineCompletion(medicine);
      setMedicineTimePopup(null);
    }
  };


  
  useEffect(() => {
    if (showMobileMenu) {
      
      const scrollY = window.scrollY;
      document.body.style.position = 'fixed';
      document.body.style.top = `-${scrollY}px`;
      document.body.style.width = '100%';
      document.body.style.overflow = 'hidden';
    } else {
      
      const scrollY = document.body.style.top;
      document.body.style.position = '';
      document.body.style.top = '';
      document.body.style.width = '';
      document.body.style.overflow = '';
      if (scrollY) {
        window.scrollTo(0, parseInt(scrollY || '0') * -1);
      }
    }

    
    return () => {
      if (!showMobileMenu) {
        document.body.style.position = '';
        document.body.style.top = '';
        document.body.style.width = '';
        document.body.style.overflow = '';
      }
    };
  }, [showMobileMenu]);

  const handleMobileMenuToggle = () => {
    setShowMobileMenu(!showMobileMenu);
  };

  const handleMobileMenuClose = () => {
    setShowMobileMenu(false);
  };

  const handleMobileMenuAction = (action) => {
    setShowMobileMenu(false);
    
    switch (action) {
      case 'add-medicine':
        handleAddCapsule();
        break;
      case 'notifications':
        handleNotificationClick();
        break;
      case 'admin':
        setShowAdminPanel(true);
        break;
      case 'profile':
        handleFeatureClick();
        break;
      case 'data':
        handleFeatureClick();
        break;
      case 'settings':
        handleFeatureClick();
        break;
      case 'logout':
        handleLogout();
        break;
      default:
        break;
    }
  };

  
  const handleLogout = async () => {
    console.log('handleLogout called, onLogout prop:', onLogout);
    
    if (isProcessing || isTransitioning || isLoggingOut) return;
    
    setIsLoggingOut(true);
    
    try {
      localStorage.removeItem('completedMedicines');
      setIsLoggingOut(false);
      if (onLogout) onLogout();
      else window.location.href = '/';
    } catch (error) {
      console.error('Logout API call failed:', error);
      setIsLoggingOut(false);
      alert('Çıkış işlemi sırasında hata oluştu.');
    }
  };

  const scrollToBottom = () => {
    if (chatMessagesRef.current) {
      const scrollElement = chatMessagesRef.current;
      
      
      scrollElement.scrollTop = scrollElement.scrollHeight;
      
      
      requestAnimationFrame(() => {
        if (chatMessagesRef.current) {
          chatMessagesRef.current.scrollTop = chatMessagesRef.current.scrollHeight;
        }
      });
      
      
      setTimeout(() => {
        if (chatMessagesRef.current) {
          chatMessagesRef.current.scrollTop = chatMessagesRef.current.scrollHeight;
        }
      }, 50);
      
      setTimeout(() => {
        if (chatMessagesRef.current) {
          chatMessagesRef.current.scrollTop = chatMessagesRef.current.scrollHeight;
        }
      }, 150);
      
      
      setTimeout(() => {
        if (chatMessagesRef.current) {
          chatMessagesRef.current.scrollTop = chatMessagesRef.current.scrollHeight;
        }
      }, 300);
    }
  };

  
  const closingMessages = [
    'Başka merak ettiğin bir şey varsa bana sorabilirsin ☺️',
    'Herhangi bir sorun varsa çekinmeden sorabilirsin 😊',
    'Başka bir konuda yardıma ihtiyacın olursa buradayım 💙',
    'İlaçlarınla ilgili başka soruların varsa sorabilirsin 🌟',
    'Merak ettiğin başka bir şey varsa bana ulaşabilirsin ✨',
    'Herhangi bir sorun yaşarsan bana danışabilirsin 🏥',
    'Başka bir şey öğrenmek istersen sorabilirsin 📚',
    'İlaçların hakkında daha fazla bilgi almak istersen sorabilirsin 💊',
    'Başka soruların varsa çekinmeden sorabilirsin 🤗',
    'Herhangi bir konuda yardıma ihtiyacın olursa buradayım 💪',
    'Başka merak ettiğin bir şey varsa bana sorabilirsin 🌈',
    'İlaçlarınla ilgili başka soruların varsa sorabilirsin 🎯',
    'Merak ettiğin başka bir şey varsa bana ulaşabilirsin 🚀',
    'Herhangi bir sorun yaşarsan bana danışabilirsin :)',
    'Başka bir şey öğrenmek istersen sorabilirsin 📖',
    'İlaçların hakkında daha fazla bilgi almak istersen sorabilirsin 🔬',
    'Başka soruların varsa çekinmeden sorabilirsin 💬',
    'Herhangi bir konuda yardıma ihtiyacın olursa buradayım 🎁',
    'Başka merak ettiğin bir şey varsa bana sorabilirsin 🌸',
    'İlaçlarınla ilgili başka soruların varsa sorabilirsin 🎨',
    'Merak ettiğin başka bir şey varsa bana ulaşabilirsin ⭐',
    'Herhangi bir sorun yaşarsan bana danışabilirsin 🎪',
    'Başka bir şey öğrenmek istersen sorabilirsin 📝',
    'İlaçların hakkında daha fazla bilgi almak istersen sorabilirsin 🔍',
    'Başka soruların varsa çekinmeden sorabilirsin 🗣️',
    'Herhangi bir konuda yardıma ihtiyacın olursa buradayım 🎭',
    'Başka merak ettiğin bir şey varsa bana sorabilirsin 🎵',
    'İlaçlarınla ilgili başka soruların varsa sorabilirsin 🎬',
    'Merak ettiğin başka bir şey varsa bana ulaşabilirsin 🎯',
    'Herhangi bir sorun yaşarsan bana danışabilirsin 🎲',
    'Başka bir şey öğrenmek istersen sorabilirsin 📊',
    'İlaçların hakkında daha fazla bilgi almak istersen sorabilirsin 🔬',
    'Başka soruların varsa çekinmeden sorabilirsin 💭',
    'Herhangi bir konuda yardıma ihtiyacın olursa buradayım 🎪',
    'Başka merak ettiğin bir şey varsa bana sorabilirsin 🌺',
    'İlaçlarınla ilgili başka soruların varsa sorabilirsin 🎨',
    'Merak ettiğin başka bir şey varsa bana ulaşabilirsin 🌟',
    'Herhangi bir sorun yaşarsan bana danışabilirsin 🛠️',
    'Başka bir şey öğrenmek istersen sorabilirsin 📚'
  ];

  
  const getRandomClosingMessage = () => {
    
    if (usedClosingMessagesRef.current.size >= closingMessages.length) {
      usedClosingMessagesRef.current.clear();
    }

    
    const unusedMessages = closingMessages.filter(
      (_, index) => !usedClosingMessagesRef.current.has(index)
    );

    
    const randomIndex = Math.floor(Math.random() * unusedMessages.length);
    const selectedMessage = unusedMessages[randomIndex];

    
    const originalIndex = closingMessages.indexOf(selectedMessage);
    usedClosingMessagesRef.current.add(originalIndex);

    return selectedMessage;
  };

  
  const bmiIdealMessages = [
    've şu an tam idealinde kilondasın',
    've ideal kilonda olduğunu görüyorum',
    've ideal kilonda olduğunu söyleyebilirim',
    've ideal kilonda olduğunu belirtmek isterim',
    've ideal kilonda olduğunu görebiliyorum'
  ];

  const bmiUnderweightMessages = [
    've ideal kilonun altındasın',
    've ideal kilonun biraz altındasın',
    've ideal kilonun altında olduğunu görüyorum',
    've ideal kilonun altında olduğunu söyleyebilirim'
  ];

  const bmiOverweightMessages = [
    've ideal kilonun üzerindesin',
    've ideal kilonun biraz üzerindesin',
    've ideal kilonun üzerinde olduğunu görüyorum',
    've ideal kilonun üzerinde olduğunu söyleyebilirim'
  ];

  const bmiObeseMessages = [
    've obezite riski taşıyorsun',
    've obezite riski altındasın',
    've obezite riski taşıdığını görüyorum',
    've obezite riski altında olduğunu söyleyebilirim'
  ];

  const bmiFormMessages = [
    'Formunu korumaya devam et',
    'Formunu korumaya devam etmelisin',
    'Formunu korumaya özen göster',
    'Formunu korumaya dikkat et',
    'Formunu korumaya çalış',
    'Formunu korumaya önem ver',
    'Formunu korumaya devam etmeni öneririm'
  ];

  const bmiClosingMessages = [
    'Sağlıklı kal! 💪',
    'Kendine iyi bak! 🌟',
    'Sağlıklı günler dilerim! 🌸',
    'Sağlıkla kal! 🍀',
    'İyi günler! ☀️',
    'Sağlıklı ol! 💚',
    'Kendine dikkat et! 🌺'
  ];

  const bmiUnderweightAdvice = [
    'Bir diyetisyene danışmanı öneririm',
    'Sağlıklı kilo almak için bir diyetisyene gitmeyi düşünebilirsin',
    'Kilo almak için bir uzmana danışmanı tavsiye ederim',
    'Sağlıklı beslenme planı için bir diyetisyene başvurmanı öneririm',
    'Kilo almak için profesyonel destek almayı düşünebilirsin',
    'Bir beslenme uzmanından yardım almanı öneririm'
  ];

  const bmiOverweightAdvice = [
    'Bir diyetisyene danışmanı öneririm',
    'Kilo vermek için bir diyetisyene gitmeyi düşünebilirsin',
    'Sağlıklı kilo vermek için bir uzmana danışmanı tavsiye ederim',
    'Düzenli egzersiz yapmayı ve bir diyetisyene başvurmanı öneririm',
    'Kilo vermek için profesyonel destek almayı düşünebilirsin',
    'Bir beslenme uzmanından yardım almanı öneririm',
    'Sağlıklı beslenme ve egzersiz planı için bir diyetisyene gitmeyi öneririm'
  ];

  const bmiObeseAdvice = [
    'Mutlaka bir diyetisyene danışmanı öneririm',
    'Obezite riski için bir diyetisyene gitmeyi düşünmelisin',
    'Sağlıklı kilo vermek için bir uzmana danışmanı şiddetle tavsiye ederim',
    'Düzenli egzersiz yapmayı ve mutlaka bir diyetisyene başvurmanı öneririm',
    'Kilo vermek için profesyonel destek almanı öneririm',
    'Bir beslenme uzmanından yardım almanı şiddetle öneririm',
    'Sağlıklı beslenme ve egzersiz planı için mutlaka bir diyetisyene gitmeyi öneririm',
    'Obezite riski için hemen bir uzmana danışmanı öneririm'
  ];

  
  const getRandomBmiMessage = (messageArray, usedSet) => {
    if (usedSet.size >= messageArray.length) {
      usedSet.clear();
    }
    const unusedMessages = messageArray.filter(
      (_, index) => !usedSet.has(index)
    );
    const randomIndex = Math.floor(Math.random() * unusedMessages.length);
    const selectedMessage = unusedMessages[randomIndex];
    const originalIndex = messageArray.indexOf(selectedMessage);
    usedSet.add(originalIndex);
    return selectedMessage;
  };

  
  const handleBMICardClick = async () => {
    
    if (isMobile && !showAiChatPopup) {
      handleAiChatPopupOpen();
      await new Promise(resolve => setTimeout(resolve, 300));
    }

    setIsTyping(true);
    bmiCardClickCountRef.current += 1;

    try {
      const response = await smtiaApi.chat.getBmiAnalysis();
      
      if (!response || !response.messages || response.messages.length === 0) {
        throw new Error('BMI analizi alınamadı');
      }

      const sendMessagePart = (text, delay = 800) => {
        return new Promise((resolve) => {
          setTimeout(() => {
            const aiMessage = {
              id: Date.now() + Math.random(),
              type: 'ai',
              text: text,
              timestamp: new Date(),
              medicineImage: null
            };
            setChatMessages(prev => [...prev, aiMessage]);
            
            requestAnimationFrame(() => {
              scrollToBottom();
            });
            setTimeout(() => {
              scrollToBottom();
            }, 100);
            resolve();
          }, delay);
        });
      };

      if (!hasUserMessage) {
        await sendMessagePart(response.messages[0] || `Merhaba ${userInfo?.name || 'Kullanıcı'},`, 800);
        setHasUserMessage(true);
        setIsTyping(false);
        await new Promise(resolve => setTimeout(resolve, 300));
        setIsTyping(true);
      }

      // Send remaining messages (skip first if already sent)
      const startIndex = hasUserMessage ? 0 : 1;
      for (let i = startIndex; i < response.messages.length; i++) {
        await sendMessagePart(response.messages[i], 800);
        setIsTyping(false);
        if (i < response.messages.length - 1) {
          await new Promise(resolve => setTimeout(resolve, 300));
          setIsTyping(true);
        }
      }

      setIsTyping(false);
    } catch (error) {
      console.error('BMI analysis API call failed:', error);
      setIsTyping(false);
      
      if (error?.status === 401) {
        alert('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        window.location.href = '/';
        return;
      }
      
      alert(error?.message || 'BMI analizi alınamadı. Lütfen tekrar deneyin.');
    }
  };

  
  const handleInteractionCardClick = async () => {
    
    if (isMobile && !showAiChatPopup) {
      handleAiChatPopupOpen();
      await new Promise(resolve => setTimeout(resolve, 300));
    }

    setIsTyping(true);

    try {
      const response = await smtiaApi.chat.getInteractionAnalysis();
      
      if (!response) {
        throw new Error('İlaç etkileşim analizi alınamadı');
      }

      if (!hasUserMessage) {
        const userName = userInfo?.name || 'Kullanıcı';
        const welcomeMessage = {
          id: Date.now(),
          type: 'ai',
          text: `Merhaba ${userName},`,
          timestamp: new Date(),
          medicineImage: null
        };
        setChatMessages(prev => [...prev, welcomeMessage]);
        setHasUserMessage(true);
        await new Promise(resolve => setTimeout(resolve, 800));
      }

      // If no medicines or single medicine, show message
      if (response.message && (!response.interactions || response.interactions.length === 0)) {
        const message = {
          id: Date.now() + 1,
          type: 'ai',
          text: response.message,
          timestamp: new Date(),
          medicineImage: null
        };
        setChatMessages(prev => [...prev, message]);
        setIsTyping(false);
        return;
      }

      // Show header message
      const headerMessage = {
        id: Date.now() + 2,
        type: 'ai',
        text: response.message || 'İlaç etkileşim risk analiziniz:',
        timestamp: new Date(),
        medicineImage: null
      };
      setChatMessages(prev => [...prev, headerMessage]);
      await new Promise(resolve => setTimeout(resolve, 600));

      // Generate table HTML from interactions
      if (response.interactions && response.interactions.length > 0) {
        let tableHTML = '<div style="overflow-x: auto; margin-top: 8px;"><table style="width: 100%; border-collapse: collapse; font-size: 13px;">';
        tableHTML += '<thead><tr style="background: #f8f9fa; border-bottom: 2px solid #e0e0e0;">';
        tableHTML += '<th style="padding: 10px; text-align: left; font-weight: 600; color: #333;">İlaç 1</th>';
        tableHTML += '<th style="padding: 10px; text-align: left; font-weight: 600; color: #333;">İlaç 2</th>';
        tableHTML += '<th style="padding: 10px; text-align: center; font-weight: 600; color: #333;">Risk %</th>';
        tableHTML += '<th style="padding: 10px; text-align: center; font-weight: 600; color: #333;">Durum</th>';
        tableHTML += '</tr></thead><tbody>';

        response.interactions.forEach((item, index) => {
          const rowColor = index % 2 === 0 ? '#ffffff' : '#f8f9fa';
          tableHTML += `<tr style="background: ${rowColor}; border-bottom: 1px solid #e0e0e0;">`;
          tableHTML += `<td style="padding: 10px; color: #333;">${item.medicine1}</td>`;
          tableHTML += `<td style="padding: 10px; color: #333;">${item.medicine2}</td>`;
          tableHTML += `<td style="padding: 10px; text-align: center; color: #333; font-weight: 500;">%${item.risk}</td>`;
          tableHTML += `<td style="padding: 10px; text-align: center;"><span style="padding: 4px 12px; border-radius: 12px; background: ${item.statusColor}; color: white; font-weight: 500; font-size: 11px;">${item.status}</span></td>`;
          tableHTML += '</tr>';
        });

        tableHTML += '</tbody></table></div>';

        const tableMessage = {
          id: Date.now() + 3,
          type: 'ai',
          text: tableHTML,
          isHTML: true,
          timestamp: new Date(),
          medicineImage: null
        };
        setChatMessages(prev => [...prev, tableMessage]);
        await new Promise(resolve => setTimeout(resolve, 600));
      }

      // Show overall status message
      if (response.overallStatus) {
        const statusMessage = {
          id: Date.now() + 4,
          type: 'ai',
          text: response.overallStatus,
          timestamp: new Date(),
          medicineImage: null
        };
        setChatMessages(prev => [...prev, statusMessage]);
      }

      setIsTyping(false);
    } catch (error) {
      console.error('Interaction analysis API call failed:', error);
      setIsTyping(false);
      
      if (error?.status === 401) {
        alert('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
        window.location.href = '/';
        return;
      }
      
      alert(error?.message || 'İlaç etkileşim analizi alınamadı. Lütfen tekrar deneyin.');
    }
  };

  
  useEffect(() => {
    if (chatMessages.length > 0) {
      
      const lastMessage = chatMessages[chatMessages.length - 1];
      const shouldScroll = lastMessage && (lastMessage.type === 'ai' || lastMessage.type === 'user');
      
      if (shouldScroll) {
        
        
        scrollToBottom();
        
        
        requestAnimationFrame(() => {
          requestAnimationFrame(() => {
            scrollToBottom();
          });
        });
        
        
        const timeoutId1 = setTimeout(() => {
          scrollToBottom();
        }, 50);
        
        const timeoutId2 = setTimeout(() => {
          scrollToBottom();
        }, 150);
        
        const timeoutId3 = setTimeout(() => {
          scrollToBottom();
        }, 300);
        
        return () => {
          clearTimeout(timeoutId1);
          clearTimeout(timeoutId2);
          clearTimeout(timeoutId3);
        };
      }
    }
  }, [chatMessages]);

  
  useEffect(() => {
    if (isTyping) {
      
      requestAnimationFrame(() => {
        requestAnimationFrame(() => {
          scrollToBottom();
        });
      });
      
      const timeoutId = setTimeout(() => {
        scrollToBottom();
      }, 100);
      
      return () => clearTimeout(timeoutId);
    }
  }, [isTyping]);

  const handleChatSubmit = (e) => {
    e.preventDefault();
    if (!chatInput.trim() || isTyping) return;

    if (!hasUserMessage) {
      setIsWelcomeFading(true);
    }

    const userMessage = {
      id: Date.now(),
      type: 'user',
      text: chatInput.trim(),
      timestamp: new Date()
    };

    setChatMessages(prev => [...prev, userMessage]);
    setChatInput('');
    setIsTyping(true);
    
    
    requestAnimationFrame(() => {
      scrollToBottom();
    });
    setTimeout(() => {
      scrollToBottom();
    }, 100);
    
    
    setTimeout(() => {
      setHasUserMessage(true);
    }, 300); 

    
    setTimeout(() => {
      if (chatInputRef.current) {
        chatInputRef.current.focus();
      }
    }, 0); 

    setTimeout(() => {
      if (chatInputRef.current) {
        chatInputRef.current.focus();
      }
    }, 50);

    setTimeout(() => {
      if (chatInputRef.current) {
        chatInputRef.current.focus();
      }
    }, 100);


    setTimeout(() => {
      scrollToBottom();
    }, 100);

    
    const sendChatMessage = async () => {
      try {
        const apiResponse = await smtiaApi.chat.send(userMessage.text);
        const aiResponse = apiResponse?.reply || 'Anladım. Devam edebilirsin.';
          
          // Create AI message with typing effect
          const aiMessageId = Date.now() + 1;
          const aiMessage = {
            id: aiMessageId,
            type: 'ai',
            text: '',
            timestamp: new Date(),
            isTyping: true
          };
          
          // Add message immediately with empty text
          setChatMessages(prev => [...prev, aiMessage]);
          setIsTyping(false);
          
          // Type out the message character by character
          const fullText = aiResponse;
          let currentIndex = 0;
          
          const typeMessage = () => {
            if (currentIndex < fullText.length) {
              const textToShow = fullText.substring(0, currentIndex + 1);
              setChatMessages(prev => prev.map(msg => 
                msg.id === aiMessageId 
                  ? { ...msg, text: textToShow, isTyping: true }
                  : msg
              ));
              currentIndex++;
              // Scroll to bottom while typing
              setTimeout(() => {
                scrollToBottom();
              }, 0);
              setTimeout(typeMessage, 30); // Adjust speed here (lower = faster)
            } else {
              // Typing complete
              setChatMessages(prev => prev.map(msg => 
                msg.id === aiMessageId 
                  ? { ...msg, text: fullText, isTyping: false }
                  : msg
              ));
              setTimeout(() => {
                scrollToBottom();
              }, 100);
            }
          };
          
          // Start typing after a small delay
          setTimeout(typeMessage, 200);
          
          
          setTimeout(() => {
            if (chatInputRef.current) {
              chatInputRef.current.focus();
            }
          }, 0); 
          
          setTimeout(() => {
            if (chatInputRef.current) {
              chatInputRef.current.focus();
            }
          }, 50);
          
          setTimeout(() => {
            if (chatInputRef.current) {
              chatInputRef.current.focus();
            }
          }, 100);
          
          
          setTimeout(() => {
            scrollToBottom();
          }, 100);
      } catch (error) {
        console.error('Chat API call failed:', error);
        setIsTyping(false);
        alert(error?.message || 'Mesaj gönderilemedi. Lütfen tekrar deneyin.');
      }
    };
    
    sendChatMessage();
  };

  const handleChatInputChange = (e) => {
    setChatInput(e.target.value);
  };

  const handleChatKeyDown = (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      e.stopPropagation();
      handleChatSubmit(e);
    }
  };

  
  const handleMedicineClick = async (medicine) => {
    
    setIsTyping(true);
    await new Promise(resolve => setTimeout(resolve, 500)); 
    
    
    let timesPerWeek = 0;
    if (medicine.schedule) {
      const selectedDays = medicine.schedule.selectedDays || [];
      const times = medicine.schedule.times || [];
      timesPerWeek = selectedDays.length * times.length;
    } else {
      
      timesPerWeek = 7;
    }

    
    try {
      
      const medicineName = medicine.name;
      
      let medicineInfo = {
        name: medicineName,
        purpose: 'Bu ilaç hakkında detaylı bilgi alınamadı.',
        image: null
      };

      
      try {
        // Use local DB search (dummy seeded) to enrich purpose text
        const data = await smtiaApi.medicines.searchLocal(medicineName, 1);
        const first = data?.items?.[0];
        if (first) {
          medicineInfo = {
            ...medicineInfo,
            purpose: first.activeIngredient
              ? `Etken madde: ${first.activeIngredient}`
              : medicineInfo.purpose
          };
        }
      } catch (apiError) {
        console.log('İlaç API hatası, varsayılan bilgiler kullanılıyor:', apiError);
      }

      
      const userName = userInfo?.name || 'Kullanıcı';

      
      const sendMessagePart = (text, delay = 800) => {
        return new Promise((resolve) => {
          setTimeout(() => {
            const aiMessage = {
              id: Date.now() + Math.random(),
              type: 'ai',
              text: text,
              timestamp: new Date(),
              medicineImage: null
            };
            setChatMessages(prev => [...prev, aiMessage]);
            
            setTimeout(() => {
              scrollToBottom();
            }, 50);
            resolve();
          }, delay);
        });
      };

      
      if (!hasUserMessage) {
        await sendMessagePart(`Merhaba ${userName},`, 800);
        setHasUserMessage(true); 
        setIsTyping(false);
        await new Promise(resolve => setTimeout(resolve, 300));
        setIsTyping(true);
      }

      
      
      await sendMessagePart(`Kullandığın ilaç **${medicine.name}** ilacı,`, 800);
      setIsTyping(false);
      await new Promise(resolve => setTimeout(resolve, 300));
      setIsTyping(true);
      
      
      await sendMessagePart(`bu ilacı haftada **${timesPerWeek}** kere alıyorsun,`, 800);
      setIsTyping(false);
      await new Promise(resolve => setTimeout(resolve, 300));
      setIsTyping(true);
      
      
      if (medicineInfo.purpose && medicineInfo.purpose !== 'Bu ilaç hakkında detaylı bilgi alınamadı.') {
        await sendMessagePart(`Bu ilacın amacı: ${medicineInfo.purpose}`, 800);
        setIsTyping(false);
        await new Promise(resolve => setTimeout(resolve, 300));
        setIsTyping(true);
      }
      
      
      const closingMessage = getRandomClosingMessage();
      await sendMessagePart(closingMessage, 800);

      
      if (medicineInfo.image) {
        setIsTyping(false);
        await new Promise(resolve => setTimeout(resolve, 500));
        setIsTyping(true);
        const imageMessage = {
          id: Date.now() + Math.random(),
          type: 'ai',
          text: '',
          timestamp: new Date(),
          medicineImage: medicineInfo.image
        };
        setChatMessages(prev => [...prev, imageMessage]);
      }

      
      setIsTyping(false);

    } catch (error) {
      console.error('İlaç bilgisi alınırken hata:', error);
      
      
      setIsTyping(false);
      await new Promise(resolve => setTimeout(resolve, 300));
      setIsTyping(true);
      
      
      const sendErrorMessagePart = (text, delay = 800) => {
        return new Promise((resolve) => {
          setTimeout(() => {
            const aiMessage = {
              id: Date.now() + Math.random(),
              type: 'ai',
              text: text,
              timestamp: new Date(),
              medicineImage: null
            };
            setChatMessages(prev => [...prev, aiMessage]);
            
            setTimeout(() => {
              scrollToBottom();
            }, 50);
            resolve();
          }, delay);
        });
      };
      
      if (!hasUserMessage) {
        const userName = userInfo?.name || 'Kullanıcı';
        await sendErrorMessagePart(`Merhaba ${userName},`, 800);
        setHasUserMessage(true); 
        setIsTyping(false);
        await new Promise(resolve => setTimeout(resolve, 300));
        setIsTyping(true);
      }
      
      
      await sendErrorMessagePart(`Kullandığın ilaç **${medicine.name}** ilacı,`, 800);
      setIsTyping(false);
      await new Promise(resolve => setTimeout(resolve, 300));
      setIsTyping(true);
      
      await sendErrorMessagePart(`bu ilacı haftada **${timesPerWeek}** kere alıyorsun,`, 800);
      setIsTyping(false);
      await new Promise(resolve => setTimeout(resolve, 300));
      setIsTyping(true);
      
      const closingMessage = getRandomClosingMessage();
      await sendErrorMessagePart(closingMessage, 800);
      setIsTyping(false);
    }
  };

  const handleAiChatPopupOpen = () => {
    setShowAiChatPopup(true);
    document.body.classList.add('modal-open');
  };

  const handleAiChatPopupClose = () => {
    setShowAiChatPopup(false);
    setIsWelcomeFading(false);
    document.body.classList.remove('modal-open');
  };


  const handleMouseDown = (e) => {
    setIsDragging(true);
    setStartX(e.pageX - e.currentTarget.offsetLeft);
    setScrollLeft(e.currentTarget.scrollLeft);
  };

  const handleMouseMove = (e) => {
    if (!isDragging) return;
    e.preventDefault();
    const x = e.pageX - e.currentTarget.offsetLeft;
    const walk = (x - startX) * 2;
    e.currentTarget.scrollLeft = scrollLeft - walk;
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  const handleMouseLeave = () => {
    setIsDragging(false);
  };

  
  const requestLocationPermission = () => {
    if (!navigator.geolocation) {
      alert('Tarayıcınız konum servislerini desteklemiyor.');
      return;
    }

    setIsLoadingPharmacies(true);
    
    navigator.geolocation.getCurrentPosition(
      async (position) => {
        const { latitude, longitude } = position.coords;
        const location = { latitude, longitude };
        
        
        try {
          const addressResponse = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${latitude}&lon=${longitude}`,
            {
              headers: {
                'User-Agent': 'IlacTakipApp/1.0'
              }
            }
          );
          
          if (addressResponse.ok) {
            const addressData = await addressResponse.json();
            const address = addressData.display_name || `${latitude.toFixed(4)}, ${longitude.toFixed(4)}`;
            setUserAddress(address);
            setPendingLocation(location);
            setShowLocationConfirm(true);
            setIsLoadingPharmacies(false);
          } else {
            
            setPendingLocation(location);
            setShowLocationConfirm(true);
            setIsLoadingPharmacies(false);
          }
        } catch (error) {
          console.error('Adres alınamadı:', error);
          setPendingLocation(location);
          setShowLocationConfirm(true);
          setIsLoadingPharmacies(false);
        }
      },
      (error) => {
        console.error('Konum hatası:', error);
        setLocationPermission('denied');
        setIsLoadingPharmacies(false);
        if (error.code === error.PERMISSION_DENIED) {
          alert('Konum izni verilmedi. Lütfen tarayıcı ayarlarından konum iznini açın.');
        } else {
          alert('Konum alınamadı. Lütfen tekrar deneyin.');
        }
      },
      {
        enableHighAccuracy: true,
        timeout: 15000,
        maximumAge: 0
      }
    );
  };

  
  const confirmLocation = () => {
    if (pendingLocation) {
      setUserLocation(pendingLocation);
      setLocationPermission('granted');
      setShowLocationConfirm(false);
      fetchNearbyPharmacies(pendingLocation.latitude, pendingLocation.longitude);
    }
  };

  
  const rejectLocation = () => {
    setPendingLocation(null);
    setShowLocationConfirm(false);
    setIsLoadingPharmacies(false);
  };

  
  const isNightTime = () => {
    const now = new Date();
    const hour = now.getHours();
    
    return hour >= 22 || hour < 8;
  };

  
  const fetchNearbyPharmacies = async (latitude, longitude) => {
    try {
      setIsLoadingPharmacies(true);
      
      let allPharmacies = [];
      
      
      const searchRadii = [500, 1000, 3000];
      
      for (const radius of searchRadii) {
        try {
          
          const query = `
            [out:json][timeout:25];
            (
              node["amenity"="pharmacy"](around:${radius},${latitude},${longitude});
              way["amenity"="pharmacy"](around:${radius},${latitude},${longitude});
              relation["amenity"="pharmacy"](around:${radius},${latitude},${longitude});
            );
            out center;
          `;

          const response = await fetch('https://overpass-api.de/api/interpreter', {
            method: 'POST',
            headers: {
              'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `data=${encodeURIComponent(query)}`
          });

          if (response.ok) {
            const data = await response.json();
            const pharmacies = data.elements
              .filter(element => {
                const lat = element.lat || element.center?.lat;
                const lon = element.lon || element.center?.lon;
                return lat && lon;
              })
              .map((element) => {
                const lat = element.lat || element.center?.lat;
                const lon = element.lon || element.center?.lon;
                const distance = calculateDistance(latitude, longitude, lat, lon);
                
                return {
                  id: element.id,
                  name: element.tags?.name || `Eczane`,
                  address: element.tags?.['addr:street'] 
                    ? `${element.tags['addr:street']}${element.tags['addr:housenumber'] ? ' No:' + element.tags['addr:housenumber'] : ''}, ${element.tags['addr:city'] || element.tags['addr:district'] || ''}`.trim().replace(/^,\s*|,\s*$/g, '')
                    : element.tags?.['addr:full'] || element.tags?.['addr:housenumber'] || 'Adres bilgisi yok',
                  phone: element.tags?.['phone'] || element.tags?.['contact:phone'] || null,
                  distance: distance,
                  latitude: lat,
                  longitude: lon,
                  openingHours: element.tags?.['opening_hours'] || null,
                  isOnDuty: element.tags?.['emergency'] === 'yes' || element.tags?.['nobetci'] === 'yes' || element.tags?.['nöbetçi'] === 'yes'
                };
              });

            
            pharmacies.forEach(pharmacy => {
              if (!allPharmacies.find(p => p.id === pharmacy.id)) {
                allPharmacies.push(pharmacy);
              }
            });

            
            if (allPharmacies.length >= 10) {
              break;
            }
          }
        } catch (error) {
          console.log(`Radius ${radius}m için API hatası:`, error);
          continue;
        }
      }

      
      allPharmacies.sort((a, b) => a.distance - b.distance);

      
      const nightTime = isNightTime();
      let filteredPharmacies = allPharmacies;

      if (nightTime) {
        
        const onDutyPharmacies = allPharmacies.filter(p => p.isOnDuty);
        const regularPharmacies = allPharmacies.filter(p => !p.isOnDuty);
        
        
        if (onDutyPharmacies.length > 0) {
          filteredPharmacies = onDutyPharmacies;
        }
      }

      
      filteredPharmacies = filteredPharmacies.slice(0, 20);

      if (filteredPharmacies.length > 0) {
        setNearbyPharmacies(filteredPharmacies);
        setIsLoadingPharmacies(false);
        return;
      }

      
      console.log('API\'den yeterli sonuç gelmedi, gerçekçi mock data kullanılıyor');
      const mockPharmacies = generateRealisticMockPharmacies(latitude, longitude);
      setNearbyPharmacies(mockPharmacies);
    } catch (error) {
      console.error('Eczane verisi alınamadı:', error);
      
      const mockPharmacies = generateRealisticMockPharmacies(latitude, longitude);
      setNearbyPharmacies(mockPharmacies);
    } finally {
      setIsLoadingPharmacies(false);
    }
  };

  
  const generateRealisticMockPharmacies = (userLat, userLon) => {
    
    const pharmacyNames = [
      'Sağlık Eczanesi', 'Modern Eczane', 'Merkez Eczanesi', 'Şifa Eczanesi', 
      'Huzur Eczanesi', 'Güven Eczanesi', 'Hayat Eczanesi', 'İdeal Eczane',
      'Yeni Eczane', 'Özel Eczane', 'Aile Eczanesi', 'Çağdaş Eczane'
    ];

    
    const streetNames = [
      'Atatürk Caddesi', 'İstiklal Caddesi', 'Cumhuriyet Bulvarı', 'Bahçelievler Mahallesi',
      'Kızılay Sokak', 'Mehmet Akif Ersoy Caddesi', 'İnönü Caddesi', 'Fevzi Çakmak Caddesi'
    ];

    const nightTime = isNightTime();
    const pharmacies = [];
    
    
    for (let i = 0; i < 5; i++) {
      const angle = (Math.PI * 2 * i) / 5;
      const distanceKm = 0.05 + Math.random() * 0.45; 
      
      const latOffset = (distanceKm * 0.009) * Math.cos(angle);
      const lonOffset = (distanceKm * 0.009) * Math.sin(angle) / Math.cos(userLat * Math.PI / 180);
      
      const lat = userLat + latOffset;
      const lon = userLon + lonOffset;
      
      pharmacies.push({
        id: `mock_pharmacy_near_${i}`,
        name: pharmacyNames[i % pharmacyNames.length],
        address: `${streetNames[i % streetNames.length]} No:${Math.floor(Math.random() * 200) + 1}`,
        phone: `0${Math.floor(Math.random() * 9) + 2}${Math.floor(Math.random() * 900) + 100} ${Math.floor(Math.random() * 9000) + 1000}`,
        distance: parseFloat(distanceKm.toFixed(3)),
        latitude: lat,
        longitude: lon,
        openingHours: '09:00-22:00',
        isOnDuty: nightTime && i < 2 
      });
    }

    
    for (let i = 0; i < 5; i++) {
      const angle = (Math.PI * 2 * i) / 5 + Math.PI / 10;
      const distanceKm = 0.5 + Math.random() * 1.5; 
      
      const latOffset = (distanceKm * 0.009) * Math.cos(angle);
      const lonOffset = (distanceKm * 0.009) * Math.sin(angle) / Math.cos(userLat * Math.PI / 180);
      
      const lat = userLat + latOffset;
      const lon = userLon + lonOffset;
      
      pharmacies.push({
        id: `mock_pharmacy_far_${i}`,
        name: pharmacyNames[(i + 5) % pharmacyNames.length],
        address: `${streetNames[(i + 5) % streetNames.length]} No:${Math.floor(Math.random() * 200) + 1}`,
        phone: `0${Math.floor(Math.random() * 9) + 2}${Math.floor(Math.random() * 900) + 100} ${Math.floor(Math.random() * 9000) + 1000}`,
        distance: parseFloat(distanceKm.toFixed(2)),
        latitude: lat,
        longitude: lon,
        openingHours: '09:00-22:00',
        isOnDuty: false
      });
    }

    
    if (nightTime) {
      const onDutyPharmacies = pharmacies.filter(p => p.isOnDuty);
      if (onDutyPharmacies.length > 0) {
        return onDutyPharmacies.sort((a, b) => a.distance - b.distance);
      }
    }

    return pharmacies.sort((a, b) => a.distance - b.distance);
  };

  
  const calculateDistance = (lat1, lon1, lat2, lon2) => {
    const R = 6371; 
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a = 
      Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
      Math.sin(dLon / 2) * Math.sin(dLon / 2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return parseFloat((R * c).toFixed(2));
  };

  
  const MapCenter = ({ center, zoom }) => {
    const map = useMap();
    useEffect(() => {
      if (center) {
        map.setView(center, zoom);
      }
    }, [map, center, zoom]);
    return null;
  };

  
  const createPharmacyIcon = (isSelected = false) => {
    return L.divIcon({
      className: 'custom-pharmacy-marker',
      html: `<div style="
        background-color: ${isSelected ? '#0466E0' : '#63B3ED'};
        width: ${isSelected ? '36px' : '30px'};
        height: ${isSelected ? '36px' : '30px'};
        border-radius: 50% 50% 50% 0;
        transform: rotate(-45deg);
        border: ${isSelected ? '4px' : '3px'} solid #ffffff;
        box-shadow: ${isSelected ? '0 4px 8px rgba(4, 102, 224, 0.5)' : '0 2px 4px rgba(99, 179, 237, 0.3)'};
        display: flex;
        align-items: center;
        justify-content: center;
      ">
        <div style="
          transform: rotate(45deg);
          color: white;
          font-size: ${isSelected ? '20px' : '18px'};
          font-weight: bold;
        ">+</div>
      </div>`,
      iconSize: [isSelected ? 36 : 30, isSelected ? 36 : 30],
      iconAnchor: [isSelected ? 18 : 15, isSelected ? 36 : 30],
      popupAnchor: [0, isSelected ? -36 : -30]
    });
  };

  
  const createUserIcon = () => {
    return L.divIcon({
      className: 'custom-user-marker',
      html: `<div style="
        background-color: #27ae60;
        width: 24px;
        height: 24px;
        border-radius: 50%;
        border: 3px solid #ffffff;
        box-shadow: 0 2px 4px rgba(0,0,0,0.3);
      "></div>`,
      iconSize: [24, 24],
      iconAnchor: [12, 12],
      popupAnchor: [0, -12]
    });
  };

 
  const handleTouchStart = (e) => {
    setIsDragging(true);
    setStartX(e.touches[0].pageX - e.currentTarget.offsetLeft);
    setScrollLeft(e.currentTarget.scrollLeft);
  };

  const handleTouchMove = (e) => {
    if (!isDragging) return;
    const x = e.touches[0].pageX - e.currentTarget.offsetLeft;
    const walk = (x - startX) * 2;
    e.currentTarget.scrollLeft = scrollLeft - walk;
  };

  const handleTouchEnd = () => {
    setIsDragging(false);
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Escape') {
      handleBack();
    } else if (e.key === 'Enter') {
      handleAddCapsule();
    }
  };

  
  useEffect(() => {
    let isMounted = true; 
    
    const loadUserInfo = async () => {
      try {
        const me = await smtiaApi.profile.me();
        console.log('[Dashboard] loadUserInfo başarılı:', me);
        const resolvedName =
          me?.name ||
          authUser?.name ||
          (userData ? `${userData.name} ${userData.surname}` : 'User');

        const resolvedEmail =
          me?.email ||
          authUser?.email ||
          (userData ? userData.email : '');

        const resolvedId = me?.id || authUser?.id || '';

        if (isMounted) {
          // Set userInfo with ALL backend data
          setUserInfo({
            id: resolvedId,
            name: resolvedName,
            email: resolvedEmail,
            userName: me?.userName || null,
            ageYears: me?.ageYears || null,
            heightCm: me?.heightCm || null,
            weightKg: me?.weightKg || null,
            gender: me?.gender || null,
            bloodType: me?.bloodType || null,
            avatar: null
          });
          
          // Load interaction risk from backend if available
          if (me?.interactionRiskPercentage != null) {
            setMedicineInteractionRisk(me.interactionRiskPercentage);
            riskLoadedFromBackendRef.current = true;
          }
          
          // Update userBodyData from backend (use userInfo data)
          if (me?.weightKg != null && me?.heightCm != null) {
            setUserBodyData({
              weight: me.weightKg,
              height: me.heightCm
            });
          } else if (userInfo?.weightKg != null && userInfo?.heightCm != null) {
            setUserBodyData({
              weight: userInfo.weightKg,
              height: userInfo.heightCm
            });
          }
          
          setIsLoadingUserInfo(false);
        }
      } catch (error) {
        if (isMounted) {
          // Don't use mock data - show real error or use authUser data
          if (error?.status === 401) {
            // Token expired or invalid - redirect to login
            alert('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
            if (onLogout) onLogout();
            return;
          }
          
          // Use authUser data if available, otherwise show error
          const fallbackUserInfo = {
            id: authUser?.id || '',
            name: authUser?.name || (userData ? `${userData.name} ${userData.surname}` : 'User'),
            email: authUser?.email || (userData ? userData.email : ''),
            userName: authUser?.userName || null,
            ageYears: null,
            heightCm: null,
            weightKg: null,
            gender: null,
            bloodType: null,
            avatar: null
          };
          setUserInfo(fallbackUserInfo);
          setIsLoadingUserInfo(false);
          
          // Show error message to user
          if (error?.message) {
            console.error('Failed to load user info:', error.message);
          }
        }
      }
    };

    const loadUserMedicines = async () => {
      if (!isMounted) return; 
      
      setIsLoadingMedicines(true);
      
      try {
        const medicines = await smtiaApi.userMedicines.list();
        if (isMounted) {
          const normalized = (medicines || []).map(m => ({
            ...m,
            schedule: {
              ...m.schedule,
              times: (m.schedule?.times || []).map(t => ({ ...t, id: t.id || `${m.id}-${t.time}` }))
            },
            usageHistory: {}
          }));
          setAddedMedicines(normalized);
          // Track initial medicine count after loading
          previousMedicineCountRef.current = normalized.length;
          setIsLoadingMedicines(false);
        }
      } catch (error) {
        if (isMounted) {
          setAddedMedicines([]);
          previousMedicineCountRef.current = 0;
          setIsLoadingMedicines(false);
        }
      }
    };

    const loadUserSideEffects = async () => {
      if (!isMounted) return;
      
      try {
        const sideEffects = await smtiaApi.sideEffects.list();
        if (isMounted) {
          // Convert backend format to frontend format
          const converted = (sideEffects || []).map(se => ({
            id: se.id,
            medicine: {
              id: se.medicine?.id,
              name: se.medicine?.name || 'Bilinmeyen İlaç',
              type: 'tablet' // Default type
            },
            severity: se.severity,
            sideEffects: Array.isArray(se.sideEffects) ? se.sideEffects : (se.sideEffects ? se.sideEffects.split(',').map(s => s.trim()) : []),
            date: se.date
          }));
          setSavedSideEffects(converted);
          // Also update localStorage for backward compatibility
          localStorage.setItem('savedSideEffects', JSON.stringify(converted));
        }
      } catch (error) {
        console.error('Error loading side effects:', error);
        // Keep existing localStorage data if API fails
      }
    };
    
    
    loadUserInfo();
    loadUserMedicines();
    loadUserSideEffects();
    
    
    const fallbackTimeout = setTimeout(() => {
      console.log('Fallback: Force loading to complete after 3 seconds');
      if (isMounted) {
        setIsLoadingMedicines(false);
        setIsLoadingUserInfo(false);
      }
    }, 3000);
    
    
    const handleResize = () => {
      setIsMobile(window.innerWidth <= 768);
      
      if (window.innerWidth > 480 && showAiChatPopup) {
        setShowAiChatPopup(false);
      }
    };

    window.addEventListener('resize', handleResize);

    
    return () => {
      isMounted = false; 
      clearTimeout(fallbackTimeout);
      window.removeEventListener('resize', handleResize);
      
      document.body.classList.remove('modal-open');
    };
  }, []); 

  
  useEffect(() => {
    setInteractionBars(generateRandomInteractionBars());
  }, []);

  // Update interaction risk when medicines change
  useEffect(() => {
    const medicineCount = addedMedicines.length;
    
    // Skip if medicine count hasn't actually changed
    if (medicineCount === previousMedicineCountRef.current) {
      return;
    }
    
    // If risk was loaded from backend and count is same, don't recalculate
    if (riskLoadedFromBackendRef.current && medicineCount === previousMedicineCountRef.current) {
      return;
    }
    
    const calculateAndSaveRisk = async () => {
      let risk = 0;
      
      if (medicineCount === 0) {
        risk = 0;
      } else if (medicineCount === 1) {
        risk = 0;
      } else if (medicineCount === 2) {
        // 2 ilaç: 5-15% arası
        risk = Math.floor(Math.random() * 11) + 5; // 5-15
      } else if (medicineCount === 3) {
        // 3 ilaç: 10-25% arası
        risk = Math.floor(Math.random() * 16) + 10; // 10-25
      } else if (medicineCount === 4) {
        // 4 ilaç: 15-30% arası
        risk = Math.floor(Math.random() * 16) + 15; // 15-30
      } else {
        // 5+ ilaç: 20-40% arası
        risk = Math.floor(Math.random() * 21) + 20; // 20-40
      }
      
      // Update local state immediately
      setMedicineInteractionRisk(risk);
      
      // Save to backend
      try {
        await smtiaApi.profile.update({
          interactionRiskPercentage: risk
        });
        console.log('Interaction risk saved to backend:', risk);
        riskLoadedFromBackendRef.current = false; // Mark as calculated, not loaded
      } catch (error) {
        console.error('Error saving interaction risk:', error);
        // Don't show error to user, just log it
      }
      
      // Update previous count
      previousMedicineCountRef.current = medicineCount;
      
      // Also update interaction bars when medicines change
      setInteractionBars(generateRandomInteractionBars());
    };
    
    // Only calculate if medicines count changed
    calculateAndSaveRisk();
  }, [addedMedicines.length]); 

  
  useEffect(() => {
    if (initialMedicines && initialMedicines.length > 0) {
      console.log('Initial medicines provided, updating state:', initialMedicines);
      setAddedMedicines(initialMedicines);
    }
  }, [initialMedicines]);

  useEffect(() => {
    window.scrollTo(0, 0);
    document.documentElement.scrollTop = 0;
    document.body.scrollTop = 0;
    
    const timer = setTimeout(() => {
      setIsPageLoaded(true);
    }, 100);
    
    const container = document.querySelector('.dashboard-container');
    if (container) {
      container.focus();
    }
    
    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    if (isTyping) {
      setTimeout(() => {
        scrollToBottom();
      }, 100);
    }
  }, [isTyping]);

  useEffect(() => {
    if (showAiChatPopup && chatInputRef.current) {
      setTimeout(() => {
        if (chatInputRef.current) {
          chatInputRef.current.focus();
        }
      }, 100);
      
      const focusInterval = setInterval(() => {
        if (chatInputRef.current && !isTyping) {
          chatInputRef.current.focus();
        }
      }, 200);
      
      return () => clearInterval(focusInterval);
    }
  }, [showAiChatPopup, isTyping]);


  return (
    <>
    <div 
      className={`dashboard-container ${isPageLoaded ? 'loaded fade-in' : ''}`}
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      <div className="dashboard-main-wrapper">

        <div className="dashboard-section">
        <div className="dashboard-welcome-section">
          {}
          <button className="welcome-hamburger-menu" onClick={handleMobileMenuToggle} aria-label="Menü">
            <img src={menuIcon} alt="Menu" className="welcome-hamburger-icon" />
          </button>
          
          {}
          <div className="welcome-text">
            <div className="welcome-title">
              <span className="welcome-label">Hoşgeldin</span>
              {!isLoadingUserInfo && <span className="welcome-comma">, </span>}
              <span className={`welcome-username ${isLoadingUserInfo ? 'loading' : 'loaded'}`}>
                {isLoadingUserInfo ? (
                  <span className="username-loading">
                    <span className="loading-dots">
                      <span></span>
                      <span></span>
                      <span></span>
                    </span>
                  </span>
                ) : (
                  <>
                    {userInfo.name.split(' ')[0]} ✨
                  </>
                )}
              </span>
            </div>
            <div className="welcome-date">
              {(() => {
                const date = new Date();
                const day = date.getDate();
                const month = date.toLocaleDateString('tr-TR', { month: 'long' });
                const weekday = date.toLocaleDateString('tr-TR', { weekday: 'long' });
                return `${day} ${month},${weekday}`;
              })()}
            </div>
          </div>
          
          {}
          <div className="welcome-header-actions">
            {}
            <div className="sensitive-data-toggle" onClick={() => setIsSensitiveDataVisible(!isSensitiveDataVisible)}>
              <div className={`toggle-switch ${isSensitiveDataVisible ? 'active' : ''}`}>
                <div className="toggle-slider">
                  {isSensitiveDataVisible ? (
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path>
                      <line x1="1" y1="1" x2="23" y2="23"></line>
                    </svg>
                  ) : (
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                      <circle cx="12" cy="12" r="3"></circle>
                    </svg>
                  )}
                </div>
              </div>
            </div>
            
            {}
            <div className="header-action-icons">
              <button className="header-icon-button" onClick={handleAddCapsule} title="İlaç Ekle">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="12" y1="5" x2="12" y2="19"></line>
                  <line x1="5" y1="12" x2="19" y2="12"></line>
                </svg>
              </button>
              
              <div className="notification-button-wrapper">
                <button className="header-icon-button" onClick={handleNotificationClick} title="Bildirimler">
                  <img src={notificationIcon} alt="Bildirimler" className="header-icon-img" />
                  {notifications.filter(n => !n.read && n.type !== 'reminder').length > 0 && (
                    <span className="notification-badge">{notifications.filter(n => !n.read && n.type !== 'reminder').length}</span>
                  )}
                </button>
                {}
                {showNotifications && (
                  <div className={`notifications-dropdown ${showNotifications ? 'show' : ''}`}>
                    <div className="notifications-dropdown-header">
                      <h3 className="notifications-dropdown-title">Bildirimler</h3>
                      <button className="notifications-dropdown-close" onClick={handleNotificationClose}>
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <line x1="18" y1="6" x2="6" y2="18"></line>
                          <line x1="6" y1="6" x2="18" y2="18"></line>
                        </svg>
                      </button>
                    </div>
                    <div className="notifications-dropdown-content">
                      {notifications.filter(n => n.type !== 'reminder').length === 0 ? (
                        <div className="notification-empty">
                          <p>Henüz bildiriminiz yok</p>
                        </div>
                      ) : (
                        notifications.filter(n => n.type !== 'reminder').map((notification) => (
                          <div
                            key={notification.id}
                            className={`notification-item ${!notification.read ? 'unread' : ''}`}
                            onClick={() => markNotificationAsRead(notification.id)}
                          >
                            <div className="notification-content">
                              <p className="notification-message">{notification.message}</p>
                              <span className="notification-time">{notification.time}</span>
                            </div>
                            <div className="notification-actions">
                              {!notification.read && <div className="notification-dot"></div>}
                              <button
                                className="notification-delete-btn"
                                onClick={(e) => deleteNotification(notification.id, e)}
                                title="Sil"
                              >
                                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                  <path d="M3 6h18"></path>
                                  <path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"></path>
                                  <path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"></path>
                                </svg>
                              </button>
                            </div>
                          </div>
                        ))
                      )}
                    </div>
                  </div>
                )}
              </div>
              
              <div className="profile-button-wrapper">
                <button className="header-icon-button header-icon-button-profile" onClick={handleProfileClick} title="Profil">
                  <img src={profileIcon} alt="Profil" className="header-icon-img" />
                </button>
                {}
                {showProfileDropdown && (
                  <div className={`profile-dropdown ${showProfileDropdown ? 'show' : ''}`}>
                    <div className="profile-dropdown-content">
                      <button 
                        className="profile-dropdown-item"
                        onClick={handleSettingsClick}
                      >
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <circle cx="12" cy="12" r="3"></circle>
                          <path d="M12 1v6m0 6v6M5.64 5.64l4.24 4.24m4.24 4.24l4.24 4.24M1 12h6m6 0h6M5.64 18.36l4.24-4.24m4.24-4.24l4.24-4.24"></path>
                        </svg>
                        <span>Ayarlar</span>
                      </button>

                      {isAdmin && (
                        <button
                          className="profile-dropdown-item"
                          onClick={() => {
                            handleProfileClose();
                            setShowAdminPanel(true);
                          }}
                        >
                          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <path d="M12 2l8 4v6c0 5-3.5 9.5-8 10-4.5-.5-8-5-8-10V6l8-4z"></path>
                            <path d="M9 12l2 2 4-4"></path>
                          </svg>
                          <span>Admin Panel</span>
                        </button>
                      )}

                      <div className="profile-dropdown-divider"></div>
                      <button 
                        className="profile-dropdown-item profile-dropdown-item-logout"
                        onClick={() => {
                          handleProfileClose();
                          handleLogout();
                        }}
                      >
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
                          <polyline points="16 17 21 12 16 7"></polyline>
                          <line x1="21" y1="12" x2="9" y2="12"></line>
                        </svg>
                        <span>Çıkış Yap</span>
                      </button>
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        <div className="dashboard-two-colm">
        <div className="dashboard-colm-1">
            {}
            <div className="health-stats-cards">
              {}
              <div 
                className={`health-stat-card bmi-card ${isSensitiveDataVisible ? 'blur-sensitive' : ''}`}
                onClick={!isSensitiveDataVisible ? handleBMICardClick : undefined}
                style={!isSensitiveDataVisible ? { cursor: 'pointer' } : {}}
              >
                <div className="health-stat-header">
                  <div className="health-stat-title">
                    <span>Vücut Yağ</span>
                    <span>İndeksin</span>
                  </div>
                  <div className="health-stat-value">{calculateBMI()}</div>
                </div>
                <div className="health-stat-bar-container">
                  <div className="health-stat-bar">
                    <div className="bar-segment" style={{ width: '20%', background: '#63B3ED' }}></div>
                    <div className="bar-segment" style={{ width: '20%', background: '#68D391' }}></div>
                    <div className="bar-segment" style={{ width: '20%', background: '#48BB78' }}></div>
                    <div className="bar-segment" style={{ width: '20%', background: '#F6AD55' }}></div>
                    <div className="bar-segment" style={{ width: '20%', background: '#FC8181' }}></div>
                  </div>
                  <div 
                    className="bar-marker" 
                    style={{ 
                      left: `${getBMICategory(calculateBMI()).position}%`,
                      backgroundColor: '#3d3d3d'
                    }}
                  ></div>
                </div>
                <div className="health-stat-status">
                  <span className="status-text">{getBMICategory(calculateBMI()).status.split(' ').slice(0, -1).join(' ')} </span>
                  <span className="status-highlight" style={{ color: getBMICategory(calculateBMI()).color }}>
                    {getBMICategory(calculateBMI()).status.split(' ').slice(-1).join(' ')}
                  </span>
                </div>
              </div>

              {}
              <div 
                className="health-stat-card interaction-card"
                onClick={handleInteractionCardClick}
                style={{ cursor: 'pointer' }}
              >
                <div className="health-stat-header">
                  <div className="health-stat-title">
                    <span>Etkileşim</span>
                    <span>Riski</span>
                  </div>
                  <div className="health-stat-value">%{medicineInteractionRisk}</div>
                </div>
                <div className="interaction-bar-chart">
                  {interactionBars.length > 0 ? (
                    interactionBars.map((bar, index) => {
                      return (
                        <div key={index} className="interaction-bar" style={{ height: '100%' }}>
                          <div className="bar-fill" style={{ 
                            height: `${bar.height}%`,
                            background: '#B9CE95',
                            border: 'none'
                          }}></div>
                          {bar.hasWhite && (
                            <div className="bar-fill-top" style={{ 
                              height: `${100 - bar.height}%`,
                              background: '#FCFFFC',
                              border: 'none'
                            }}></div>
                          )}
                        </div>
                      );
                    })
                  ) : (
                    
                    Array.from({ length: 13 }).map((_, index) => (
                      <div key={index} className="interaction-bar" style={{ height: '100%' }}>
                        <div className="bar-fill" style={{ 
                          height: '50%',
                          background: '#B9CE95',
                          border: 'none'
                        }}></div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </div>
            
            <div className="dashboard-user-capsule-time">
              <div className={`daily-medicine-section ${isSensitiveDataVisible ? 'blur-sensitive' : ''}`}>
                {}
                <div className="day-selection">
                  {getDaysAroundToday().map((dayInfo, index) => {
                    const isToday = dayInfo.isToday;
                    const isSelected = selectedDate.toDateString() === dayInfo.date.toDateString();
                    const isPast = dayInfo.date < new Date();
                    const isFuture = dayInfo.date > new Date();
                    
                    return (
                      <button
                        key={index}
                        className={`day-button ${isToday ? 'today' : ''} ${isSelected && !isToday ? 'selected' : ''} ${isPast ? 'past' : ''} ${isFuture ? 'future' : ''}`}
                        onClick={() => handleDaySelect(dayInfo)}
                        title={`${getDayFullName(dayInfo.dayIndex)} - ${dayInfo.dateString}`}
                      >
                        <span className="day-letter">{dayInfo.dayName}</span>
                        <span className="day-date">{dayInfo.dateString}</span>
                      </button>
                    );
                  })}
                </div>

                {}
                <div className="daily-medicine-list">
                  {getMedicinesForDay(selectedDay).map((medicine, index) => {
                    const timePassed = isMedicineTimePassed(medicine);
                    return (
                    <div 
                      key={medicine.id} 
                      className={`daily-medicine-item ${isMedicineCompleted(medicine) ? 'completed' : ''} ${timePassed ? 'time-passed' : ''} ${medicineMessageSent[medicine.id] === 'sent' ? 'already-consulted' : ''} ${medicineAIDecision[medicine.id] === true ? 'can-take-item' : ''}`}
                      style={{
                        transitionDelay: `${index * 30}ms`
                      }}
                      onClick={() => {
                        
                        if (medicineMessageSent[medicine.id] === 'sent') {
                          return;
                        }
                        if (timePassed) {
                          handleMedicineItemClick(medicine);
                        }
                      }}
                    >
                      <div className="medicine-icon-container">
                        <img 
                          src={getMedicineIcon(medicine.type)} 
                          alt={medicine.name} 
                          className="daily-medicine-icon" 
                        />
                      </div>
                      
                      <div className="medicine-info">
                        <div className="medicine-name-row">
                          <div className={`medicine-name ${isMedicineCompleted(medicine) ? 'completed' : ''}`}>
                            {medicine.name}
                          </div>
                          {isMedicineCompleted(medicine) && (
                            <span className="completed-badge">İçildi</span>
                          )}
                          {timePassed && !isMedicineCompleted(medicine) && (
                            <>
                              <span className="time-passed-badge">Saati Geçti</span>
                              {medicineAIDecision[medicine.id] !== undefined && (
                                <span className={`ai-decision-badge ${medicineAIDecision[medicine.id] ? 'can-take' : 'cannot-take'}`}>
                                  {medicineAIDecision[medicine.id] ? (
                                    <>
                                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
                                        <polyline points="20,6 9,17 4,12"></polyline>
                                      </svg>
                                      Şu anda içilebilir
                                    </>
                                  ) : (
                                    <>
                                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
                                        <line x1="18" y1="6" x2="6" y2="18"></line>
                                        <line x1="6" y1="6" x2="18" y2="18"></line>
                                      </svg>
                                      İçilemez
                                    </>
                                  )}
                                </span>
                              )}
                            </>
                          )}
                        </div>
                        <div className="medicine-details">
                          <span className="dosage">{medicine.dosage}</span>
                          <span className="separator">•</span>
                          <span className="time">{medicine.times[0]?.time || '08:00'}</span>
                          {timePassed && !isMedicineCompleted(medicine) && (
                            <span className={`time-passed-hint ${medicineMessageSent[medicine.id] === 'sending' ? 'sending' : ''} ${medicineMessageSent[medicine.id] === 'sent' ? 'sent' : ''}`}>
                              {medicineMessageSent[medicine.id] === 'sending' && (
                                <span className="hint-sending">
                                  <span className="sending-dots">
                                    <span>.</span><span>.</span><span>.</span>
                                  </span>
                                  Gönderiliyor
                                </span>
                              )}
                              {medicineMessageSent[medicine.id] === 'sent' && (
                                <span className="hint-sent">
                                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
                                    <path d="M12 2L2 7l10 5 10-5-10-5z"></path>
                                    <path d="M2 17l10 5 10-5"></path>
                                    <path d="M2 12l10 5 10-5"></path>
                                  </svg>
                                  Yapay zekaya danışıldı
                                </span>
                              )}
                              {!medicineMessageSent[medicine.id] && (
                                <span className="hint-text">Yapay Zekaya danış?</span>
                              )}
                            </span>
                          )}
                        </div>
                      </div>
                      
                      <div 
                        className="medicine-checkbox"
                        onClick={(e) => {
                          e.preventDefault();
                          e.stopPropagation();
                          
                          const cannotTake = medicineAIDecision[medicine.id] === false;
                          if (cannotTake && medicineMessageSent[medicine.id] === 'sent') {
                            return;
                          }
                          
                          if (isMedicineCompleted(medicine)) {
                            return;
                          }
                          console.log('Checkbox container clicked!', medicine);
                          if (!isProcessing && !isTransitioning) {
                            handleMedicineCompletion(medicine);
                          }
                        }}
                      >
                        <button
                          className={`checkbox ${isMedicineCompleted(medicine) ? 'checked' : ''} ${medicineAIDecision[medicine.id] === false && medicineMessageSent[medicine.id] === 'sent' ? 'disabled' : ''}`}
                          onClick={(e) => {
                            e.preventDefault();
                            e.stopPropagation();
                            
                            const cannotTake = medicineAIDecision[medicine.id] === false;
                            if (cannotTake && medicineMessageSent[medicine.id] === 'sent') {
                              return;
                            }
                            
                            if (isMedicineCompleted(medicine)) {
                              return;
                            }
                            console.log('Checkbox button clicked!', medicine);
                            handleMedicineCompletion(medicine);
                          }}
                          disabled={isProcessing || isTransitioning || (medicineAIDecision[medicine.id] === false && medicineMessageSent[medicine.id] === 'sent') || isMedicineCompleted(medicine)}
                        >
                          {isMedicineCompleted(medicine) && (
                            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                              <polyline points="20,6 9,17 4,12"/>
                            </svg>
                          )}
                        </button>
                      </div>
                    </div>
                    );
                  })}
                  
                  {getMedicinesForDay(selectedDay).length === 0 && (
                    <div className="no-medicines-message">
                      <p>Bu gün için ilaç bulunmuyor</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
            
            {}
            <div className="dashboard-nearby-pharmacies">
              <div className="nearby-pharmacies-section">
                <div className="nearby-pharmacies-header">
                  <div className="nearby-pharmacies-title-container">
                    <h2 className="nearby-pharmacies-title">Yakınımdaki Eczaneler</h2>
                  </div>
                  <div className="pharmacies-header-actions">
                    {nearbyPharmacies.length > 0 && (
                      <div className="view-mode-toggle">
                        <button 
                          className={`view-mode-button ${viewMode === 'list' ? 'active' : ''}`}
                          onClick={() => setViewMode('list')}
                          title="Sadece Liste"
                        >
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <line x1="8" y1="6" x2="21" y2="6"></line>
                            <line x1="8" y1="12" x2="21" y2="12"></line>
                            <line x1="8" y1="18" x2="21" y2="18"></line>
                            <line x1="3" y1="6" x2="3.01" y2="6"></line>
                            <line x1="3" y1="12" x2="3.01" y2="12"></line>
                            <line x1="3" y1="18" x2="3.01" y2="18"></line>
                          </svg>
                        </button>
                        <button 
                          className={`view-mode-button ${viewMode === 'map' ? 'active' : ''}`}
                          onClick={() => setViewMode('map')}
                          title="Sadece Harita"
                        >
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <polygon points="1 6 1 22 8 18 16 22 23 18 23 2 16 6 8 2 1 6"></polygon>
                            <line x1="8" y1="2" x2="8" y2="18"></line>
                            <line x1="16" y1="6" x2="16" y2="22"></line>
                          </svg>
                        </button>
                        <button 
                          className={`view-mode-button ${viewMode === 'both' ? 'active' : ''}`}
                          onClick={() => setViewMode('both')}
                          title="Liste ve Harita"
                        >
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <rect x="3" y="3" width="7" height="7"></rect>
                            <rect x="14" y="3" width="7" height="7"></rect>
                            <rect x="14" y="14" width="7" height="7"></rect>
                            <rect x="3" y="14" width="7" height="7"></rect>
                          </svg>
                        </button>
                      </div>
                    )}
                    {!userLocation && (
                      <button 
                        className="location-permission-button"
                        onClick={requestLocationPermission}
                        disabled={isLoadingPharmacies}
                      >
                        {isLoadingPharmacies ? (
                          <>
                            <div className="spinner"></div>
                            <span>Yükleniyor...</span>
                          </>
                        ) : (
                          <>
                            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                              <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                              <circle cx="12" cy="10" r="3"></circle>
                            </svg>
                            <span>Konumumu Kullan</span>
                          </>
                        )}
                      </button>
                    )}
                  </div>
                </div>

                <div className="nearby-pharmacies-content-wrapper">
                  {isLoadingPharmacies ? (
                    <div className="pharmacies-loading">
                      <div className="medicine-loading-spinner">
                        <div className="spinner"></div>
                      </div>
                      <div className="medicine-loading-text">Eczaneler aranıyor...</div>
                    </div>
                  ) : nearbyPharmacies.length > 0 ? (
                    <>
                      {isNightTime() && nearbyPharmacies.some(p => p.isOnDuty) && (
                        <div className="night-duty-notice">
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                            <circle cx="12" cy="12" r="10"></circle>
                            <polyline points="12 6 12 12 16 14"></polyline>
                          </svg>
                          <span>Sadece nöbetçi eczaneler gösteriliyor</span>
                        </div>
                      )}
                      {(viewMode === 'map' || viewMode === 'both') && userLocation && (
                        <div className="pharmacies-map-container">
                          <MapContainer
                            center={selectedPharmacy 
                              ? [selectedPharmacy.latitude, selectedPharmacy.longitude]
                              : [userLocation.latitude, userLocation.longitude]}
                            zoom={selectedPharmacy ? 16 : 13}
                            style={{ height: viewMode === 'both' ? '300px' : '500px', width: '100%', borderRadius: '12px', zIndex: 1 }}
                            scrollWheelZoom={true}
                          >
                            <TileLayer
                              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                            />
                            <MapCenter 
                              center={selectedPharmacy 
                                ? [selectedPharmacy.latitude, selectedPharmacy.longitude]
                                : [userLocation.latitude, userLocation.longitude]} 
                              zoom={selectedPharmacy ? 16 : 13} 
                            />
                            
                            {}
                            <Marker 
                              position={[userLocation.latitude, userLocation.longitude]}
                              icon={createUserIcon()}
                            >
                              <Popup>
                                <div style={{ fontWeight: 'bold', color: '#27ae60' }}>Konumunuz</div>
                              </Popup>
                            </Marker>

                            {}
                            {nearbyPharmacies.map((pharmacy) => (
                              <Marker
                                key={pharmacy.id}
                                position={[pharmacy.latitude, pharmacy.longitude]}
                                icon={createPharmacyIcon(selectedPharmacy?.id === pharmacy.id)}
                              >
                                <Popup>
                                  <div style={{ minWidth: '200px' }}>
                                    <div style={{ fontWeight: 'bold', marginBottom: '8px', color: '#2c3e50' }}>
                                      {pharmacy.name}
                                    </div>
                                    <div style={{ fontSize: '13px', color: '#7f8c8d', marginBottom: '4px' }}>
                                      {pharmacy.address}
                                    </div>
                                    {pharmacy.phone && (
                                      <a 
                                        href={`tel:${pharmacy.phone.replace(/\s+/g, '').replace(/[()]/g, '')}`}
                                        style={{ 
                                          fontSize: '13px', 
                                          color: '#0466E0', 
                                          marginBottom: '4px',
                                          textDecoration: 'none',
                                          display: 'block',
                                          cursor: 'pointer'
                                        }}
                                        onMouseEnter={(e) => e.target.style.textDecoration = 'underline'}
                                        onMouseLeave={(e) => e.target.style.textDecoration = 'none'}
                                      >
                                        📞 {pharmacy.phone}
                                      </a>
                                    )}
                                    <div style={{ fontSize: '12px', color: '#27ae60', fontWeight: '600', marginTop: '8px' }}>
                                      📍 {pharmacy.distance < 1 
                                        ? `${(pharmacy.distance * 1000).toFixed(0)} m` 
                                        : `${pharmacy.distance.toFixed(2)} km`} uzaklıkta
                                    </div>
                                    {pharmacy.isOnDuty && (
                                      <div style={{ fontSize: '11px', color: '#e74c3c', fontWeight: '600', marginTop: '4px' }}>
                                        🏥 Nöbetçi Eczane
                                      </div>
                                    )}
                                  </div>
                                </Popup>
                              </Marker>
                            ))}
                          </MapContainer>
                        </div>
                      )}
                      {(viewMode === 'list' || viewMode === 'both') && (
                        <div className="pharmacies-list-container">
                          <div className="pharmacies-list">
                            {nearbyPharmacies.map((pharmacy) => (
                              <div 
                                key={pharmacy.id} 
                                className={`pharmacy-item ${pharmacy.isOnDuty ? 'on-duty' : ''} ${selectedPharmacy?.id === pharmacy.id ? 'selected' : ''}`}
                                onClick={() => {
                                  setSelectedPharmacy(pharmacy);
                                  
                                  if (viewMode === 'list') {
                                    setViewMode('both');
                                  }
                                }}
                                style={{ cursor: 'pointer' }}
                              >
                                <div className="pharmacy-info">
                                  <div className="pharmacy-name-row">
                                    <div className="pharmacy-name">{pharmacy.name}</div>
                                    {pharmacy.isOnDuty && (
                                      <span className="on-duty-badge">Nöbetçi</span>
                                    )}
                                  </div>
                                  <div className="pharmacy-address">{pharmacy.address}</div>
                                  {pharmacy.phone && (
                                    <a 
                                      href={`tel:${pharmacy.phone.replace(/\s+/g, '').replace(/[()]/g, '')}`}
                                      className="pharmacy-phone"
                                      onClick={(e) => e.stopPropagation()}
                                    >
                                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                        <path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"></path>
                                      </svg>
                                      <span>{pharmacy.phone}</span>
                                    </a>
                                  )}
                                </div>
                                <div className="pharmacy-distance">
                                  <span className="distance-value">
                                    {pharmacy.distance < 1 
                                      ? (pharmacy.distance * 1000).toFixed(0) 
                                      : pharmacy.distance.toFixed(2)}
                                  </span>
                                  <span className="distance-unit">
                                    {pharmacy.distance < 1 ? 'm' : 'km'}
                                  </span>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                    </>
                  ) : userLocation ? (
                    <div className="no-pharmacies-message">
                      <p>Yakınınızda eczane bulunamadı</p>
                    </div>
                  ) : (
                    <div className="no-pharmacies-message">
                      <p>Yakınındaki eczaneleri görmek için konum izni verin</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
            
            <div className={`dashboard-add-capsule ${isSensitiveDataVisible ? 'blur-sensitive' : ''}`}>
              <div className="dashboard-medicine-section">
                <div className="dashboard-medicine-header">
                  <h2 className="dashboard-medicine-title">İlaçlarım</h2>
                  <p className="dashboard-medicine-subtitle">kullandığınız ilaçlarınızın genel listesi</p>
                </div>
                
                <div className="dashboard-medicine-slider-container">
                  {}
                  <div 
                    className="dashboard-medicine-slider"
                    onMouseDown={handleMouseDown}
                    onMouseMove={handleMouseMove}
                    onMouseUp={handleMouseUp}
                    onMouseLeave={handleMouseLeave}
                    onTouchStart={handleTouchStart}
                    onTouchMove={handleTouchMove}
                    onTouchEnd={handleTouchEnd}
                  >
                    <div className="dashboard-medicine-track">
                      {}
                      {isLoadingMedicines ? (
                        <div className="dashboard-medicine-loading">
                          <div className="medicine-loading-spinner">
                            <div className="spinner"></div>
                          </div>
                          <div className="medicine-loading-text">İlaçlar yükleniyor...</div>
                        </div>
                      ) : (
                        <>
                          {}
                          {addedMedicines.map((medicine, index) => (
                            <div 
                              key={index} 
                              className="dashboard-medicine-item"
                              onClick={() => handleMedicineClick(medicine)}
                              style={{ cursor: 'pointer' }}
                            >
                              <button 
                                className="dashboard-medicine-remove-button"
                                onClick={(e) => {
                                  e.stopPropagation();
                                  handleRemoveMedicine(index);
                                }}
                                title="İlacı kaldır"
                              >
                                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                  <line x1="18" y1="6" x2="6" y2="18"/>
                                  <line x1="6" y1="6" x2="18" y2="18"/>
                                </svg>
                              </button>
                              <img src={getMedicineIcon(medicine.type)} alt={medicine.name} className="dashboard-medicine-icon" />
                              <div className="dashboard-medicine-name">{medicine.name}</div>
                            </div>
                          ))}
                        </>
                      )}
                    </div>
                  </div>
                  
                </div>
              </div>
            </div>
            
            {}
            <div className="health-stats-cards health-stats-cards-bottom">
              {}
              <div 
                className={`health-stat-card side-effect-card ${isSensitiveDataVisible ? 'blur-sensitive' : ''} ${savedSideEffects.length === 0 ? 'disabled' : ''}`}
                onClick={handleSideEffectCardClick}
                style={{ cursor: savedSideEffects.length > 0 ? 'pointer' : 'default' }}
              >
                <div className="health-stat-header">
                  <div className="health-stat-title">
                    <span>İlaç Yan</span>
                    <span>Etkisi</span>
                  </div>
                  <div className="health-stat-value side-effect-value">{savedSideEffects.length}/{addedMedicines.length}</div>
                </div>
                <div className="side-effect-legend">
                  <div className="legend-item">
                    <div className="legend-dot legend-dot-blue"></div>
                    <span className="legend-text">Yan Etkisi Olmayan</span>
                  </div>
                  <div className="legend-item">
                    <div className="legend-dot legend-dot-pink"></div>
                    <span className="legend-text">Yan Etkisi Olan</span>
                  </div>
                </div>
                <div className="side-effect-progress-bar">
                  {addedMedicines.length > 0 ? (
                    <>
                      <div className="progress-segment progress-segment-blue" style={{ width: `${((addedMedicines.length - savedSideEffects.length) / addedMedicines.length) * 100}%` }}></div>
                      <div className="progress-segment progress-segment-pink" style={{ width: `${(savedSideEffects.length / addedMedicines.length) * 100}%` }}></div>
                    </>
                  ) : (
                    <>
                      <div className="progress-segment progress-segment-blue" style={{ width: '100%' }}></div>
                    </>
                  )}
                </div>
              </div>

              {}
              <div 
                className="health-stat-card health-card"
                onClick={handleHealthCardClick}
              >
                <div className="health-stat-header">
                  <div className="health-stat-title">
                    <span>Sağlık</span>
                    <span>Kartım</span>
                  </div>
                  <div className="health-card-icon">
                    <img src={healthyIcon} alt="Sağlık Kartı" />
                  </div>
                </div>
                <div className="health-card-button-container">
                  <button className="health-card-view-button" onClick={(e) => { e.stopPropagation(); handleHealthCardClick(); }}>Görüntüle</button>
                </div>
              </div>
            </div>
        </div>
        <div className="dashboard-colm-2">

          <div className="ai-chat-section">
            <div className={`ai-chat-header ${hasUserMessage ? 'compact' : 'centered'}`}>
              <img src={logoAI} alt="AI Logo" className="ai-logo" />
              <div className="ai-chat-title">
                <p>ilaç kullanımı hakkında merak ettiklerini veya dikkat</p>
                <p>etmen gerekenleri öğrenmek için bana</p>
                <p>danışabilirsin.</p>
              </div>
            </div>

            <div className="chat-messages" ref={chatMessagesRef}>
              {chatMessages.map((message, index) => {
                const aiMessageIndices = chatMessages.map((msg, idx) => msg.type === 'ai' ? idx : -1).filter(idx => idx !== -1);
                const lastAiMessageIndex = aiMessageIndices[aiMessageIndices.length - 1];
                const isLastAiMessage = message.type === 'ai' && index === lastAiMessageIndex;
                
                return (
                  <div key={message.id} className={`chat-message ${message.type}`}>
                    {message.type === 'ai' && (
                      <div className={`ai-avatar ${isLastAiMessage ? 'has-shadow' : ''}`}>
                        <img src={logoAI} alt="AI" className="ai-avatar-img" />
                      </div>
                    )}
                    <div className="message-content">
                      {message.medicineImage && (
                        <div className="message-medicine-image">
                          <img src={message.medicineImage} alt={message.text} style={{ maxWidth: '200px', borderRadius: '8px', marginBottom: '8px' }} />
                        </div>
                      )}
                      <div className="message-text">
                        {message.isHTML ? (
                          <div dangerouslySetInnerHTML={{ __html: message.text }} />
                        ) : (
                          message.text.split('**').map((part, i) => 
                            i % 2 === 1 ? <strong key={i}>{part}</strong> : part
                          )
                        )}
                      </div>
                      <div className="message-time">
                        {message.timestamp.toLocaleTimeString('tr-TR', { 
                          hour: '2-digit', 
                          minute: '2-digit' 
                        })}
                      </div>
                    </div>
                  </div>
                );
              })}
              
              {isTyping && (
                <div className="chat-message ai">
                  <div className="ai-avatar has-shadow">
                    <img src={logoAI} alt="AI" className="ai-avatar-img" />
                  </div>
                  <div className="message-content">
                    <div className="typing-indicator">
                      <span></span>
                      <span></span>
                      <span></span>
                    </div>
                  </div>
                </div>
              )}
            </div>

         
            <form className="chat-input-form" onSubmit={handleChatSubmit}>
              <div className="chat-input-container">
                <input
                  ref={chatInputRef}
                  type="text"
                  value={chatInput}
                  onChange={handleChatInputChange}
                  onKeyDown={handleChatKeyDown}
                  placeholder="Mesajınızı yazın..."
                  className="chat-input"
                  disabled={isTyping}
                />
                <button 
                  type="submit" 
                  className="chat-send-button"
                  disabled={!chatInput.trim() || isTyping}
                >
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M22 2L11 13"/>
                    <path d="M22 2L15 22L11 13L2 9L22 2Z"/>
                  </svg>
                </button>
              </div>
            </form>
          </div>
        </div>




        </div>


        </div>

      </div>

      {}
      {showMobileMenu && (
        <div className="mobile-menu-overlay" onClick={handleMobileMenuClose}>
          <div className="mobile-menu-popup" onClick={(e) => e.stopPropagation()}>
            <div className="mobile-menu-header">
              <h3 className="mobile-menu-title">Menü</h3>
              <button 
                className="mobile-menu-close"
                onClick={handleMobileMenuClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"/>
                  <line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
              </button>
            </div>
            
            <div className="mobile-menu-items">
              {}
              <div 
                className="mobile-menu-item"
                onClick={() => handleMobileMenuAction('add-medicine')}
              >
                <div className="mobile-menu-icon-wrapper">
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <line x1="12" y1="5" x2="12" y2="19"></line>
                    <line x1="5" y1="12" x2="19" y2="12"></line>
                  </svg>
                </div>
                <span className="mobile-menu-text">İlaç Ekle</span>
              </div>
              
              {}
              <div 
                className="mobile-menu-item mobile-menu-item-notifications"
                onClick={() => handleMobileMenuAction('notifications')}
              >
                <div className="mobile-menu-icon-wrapper">
                  <img src={notificationIcon} alt="Bildirimler" className="mobile-menu-icon" />
                  {notifications.filter(n => !n.read && n.type !== 'reminder').length > 0 && (
                    <span className="mobile-menu-notification-badge">{notifications.filter(n => !n.read && n.type !== 'reminder').length}</span>
                  )}
                </div>
                <span className="mobile-menu-text">Bildirimler</span>
              </div>

              {isAdmin && (
                <div
                  className="mobile-menu-item"
                  onClick={() => handleMobileMenuAction('admin')}
                >
                  <div className="mobile-menu-icon-wrapper">
                    <img src={navbarIcon5} alt="Admin" className="mobile-menu-icon" />
                  </div>
                  <span className="mobile-menu-text">Admin Panel</span>
                </div>
              )}
              
              {}
              <div 
                className="mobile-menu-item mobile-menu-logout"
                onClick={() => handleMobileMenuAction('logout')}
              >
                <img src={navbarIcon4} alt="Çıkış" className="mobile-menu-icon" />
                <span className="mobile-menu-text">Çıkış</span>
              </div>
            </div>
          </div>
        </div>
      )}

      {showAdminPanel && (
        <AdminPanel onClose={() => setShowAdminPanel(false)} />
      )}

      {}
      {showFeaturePopup && (
        <div className="feature-popup-overlay" onClick={handleFeaturePopupClose}>
          <div className="feature-popup" onClick={(e) => e.stopPropagation()}>
            <div className="feature-popup-header">
              <h3 className="feature-popup-title">Bilgilendirme</h3>
              <button 
                className="feature-popup-close"
                onClick={handleFeaturePopupClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"/>
                  <line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
              </button>
            </div>
            <div className="feature-popup-content">
              <div className="feature-popup-icon">
                <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <circle cx="12" cy="12" r="10"/>
                  <path d="M12 6v6l4 2"/>
                </svg>
              </div>
              <p className="feature-popup-message">
                Bu özellik proje kapsamı dışında tutulmuş olup, yalnızca görsel amaçla eklenmiştir.
              </p>
              <button 
                className="feature-popup-button"
                onClick={handleFeaturePopupClose}
              >
                Anladım
              </button>
            </div>
          </div>
        </div>
      )}

      {}
      {showSettingsModal && (
        <div className="settings-modal-overlay" onClick={handleSettingsClose}>
          <div className="settings-modal" onClick={(e) => e.stopPropagation()}>
            <div className="settings-modal-header">
              <h3 className="settings-modal-title">Ayarlar</h3>
              <button 
                className="settings-modal-close"
                onClick={handleSettingsClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"></line>
                  <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
              </button>
            </div>
            <div className="settings-modal-content">
              <button 
                className="settings-modal-item"
                onClick={handleExtraHealthClick}
              >
                <div className="settings-modal-item-icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                    <polyline points="14 2 14 8 20 8"></polyline>
                    <line x1="12" y1="18" x2="12" y2="12"></line>
                    <line x1="9" y1="15" x2="15" y2="15"></line>
                  </svg>
                </div>
                <div className="settings-modal-item-text">
                  <span className="settings-modal-item-title">Ekstra Sağlık Bilgilerini Ekle</span>
                </div>
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="settings-modal-item-arrow">
                  <polyline points="9 18 15 12 9 6"></polyline>
                </svg>
              </button>
              
              <button 
                className="settings-modal-item"
                onClick={handleSideEffectClick}
              >
                <div className="settings-modal-item-icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                    <line x1="12" y1="8" x2="12" y2="16"></line>
                    <line x1="8" y1="12" x2="16" y2="12"></line>
                  </svg>
                </div>
                <div className="settings-modal-item-text">
                  <span className="settings-modal-item-title">Yan Etki Yapan İlaç Ekle</span>
                </div>
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="settings-modal-item-arrow">
                  <polyline points="9 18 15 12 9 6"></polyline>
                </svg>
              </button>
              
              <button 
                className="settings-modal-item"
                onClick={handleWeightHeightClick}
              >
                <div className="settings-modal-item-icon">
                  <img src={weightIcon} alt="Kilo Boy" />
                </div>
                <div className="settings-modal-item-text">
                  <span className="settings-modal-item-title">Kilo Boy Düzenle</span>
                </div>
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="settings-modal-item-arrow">
                  <polyline points="9 18 15 12 9 6"></polyline>
                </svg>
              </button>
            </div>
          </div>
        </div>
      )}

      {}
      {showWeightHeightModal && (
        <div className="weight-height-modal-overlay" onClick={handleWeightHeightClose}>
          <div className="weight-height-modal" onClick={(e) => e.stopPropagation()}>
            <div className="weight-height-modal-header">
              <h3 className="weight-height-modal-title">Kilo Boy Düzenle</h3>
              <button 
                className="weight-height-modal-close"
                onClick={handleWeightHeightClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"></line>
                  <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
              </button>
            </div>
            <div className="weight-height-modal-content">
              <div className="weight-height-field">
                <label className="weight-height-label">Kilo (kg)</label>
                <input
                  type="number"
                  className="weight-height-input"
                  value={weightInput}
                  onChange={(e) => setWeightInput(e.target.value)}
                  onFocus={(e) => e.target.select()}
                  placeholder="Kilo"
                  min="1"
                  step="0.1"
                />
                {calculateWeightChange() && (
                  <div className="weight-height-change">
                    {calculateWeightChange()}
                  </div>
                )}
              </div>
              
              <div className="weight-height-field">
                <label className="weight-height-label">Boy (cm)</label>
                <input
                  type="number"
                  className="weight-height-input"
                  value={heightInput}
                  onChange={(e) => setHeightInput(e.target.value)}
                  onFocus={(e) => e.target.select()}
                  placeholder="Boy"
                  min="1"
                  step="1"
                />
                {calculateHeightChange() && (
                  <div className="weight-height-change">
                    {calculateHeightChange()}
                  </div>
                )}
              </div>
            </div>
            <div className="weight-height-modal-footer">
              <button 
                className="weight-height-save-button"
                onClick={handleWeightHeightSave}
                disabled={isSaving || !weightInput || !heightInput || parseFloat(weightInput) <= 0 || parseFloat(heightInput) <= 0}
              >
                {isSaving ? 'Kaydediliyor...' : 'Kaydet'}
              </button>
            </div>
          </div>
        </div>
      )}

      {}
      {showSideEffectModal && (
        <div className="side-effect-modal-overlay" onClick={handleSideEffectClose}>
          <div className="side-effect-modal" onClick={(e) => e.stopPropagation()}>
            <div className="side-effect-modal-header">
              <h3 className="side-effect-modal-title">Yan Etki Yapan İlaç Ekle</h3>
              <button 
                className="side-effect-modal-close"
                onClick={handleSideEffectClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"></line>
                  <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
              </button>
            </div>
            <div className="side-effect-modal-content">
              {}
              <div className="side-effect-section">
                <label className="side-effect-section-label">İlaç Seç</label>
                <div className="side-effect-medicine-list">
                  {addedMedicines.length === 0 ? (
                    <div className="side-effect-empty">Henüz ilaç eklenmemiş</div>
                  ) : (
                    addedMedicines.map((medicine, index) => {
                      const isSelected = selectedMedicineForSideEffect && (
                        (selectedMedicineForSideEffect.id && selectedMedicineForSideEffect.id === medicine.id) ||
                        (!selectedMedicineForSideEffect.id && selectedMedicineForSideEffect === medicine)
                      );
                      return (
                        <button
                          key={medicine.id || index}
                          className={`side-effect-medicine-item ${isSelected ? 'selected' : ''}`}
                          onClick={() => setSelectedMedicineForSideEffect(medicine)}
                        >
                          <img src={getMedicineIcon(medicine.type)} alt={medicine.name} className="side-effect-medicine-icon" />
                          <span className="side-effect-medicine-name">{medicine.name}</span>
                          {isSelected && (
                            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="side-effect-check-icon">
                              <polyline points="20 6 9 17 4 12"></polyline>
                            </svg>
                          )}
                        </button>
                      );
                    })
                  )}
                </div>
              </div>

              {}
              {selectedMedicineForSideEffect && (
                <div className="side-effect-section">
                  <label className="side-effect-section-label">Yan Etki Şiddeti</label>
                  <div className="side-effect-severity-list">
                    {severityOptions.map((option) => (
                      <button
                        key={option.value}
                        className={`side-effect-severity-item ${selectedSeverity === option.value ? 'selected' : ''}`}
                        onClick={() => setSelectedSeverity(option.value)}
                      >
                        <span>{option.label}</span>
                        {selectedSeverity === option.value && (
                          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="side-effect-check-icon">
                            <polyline points="20 6 9 17 4 12"></polyline>
                          </svg>
                        )}
                      </button>
                    ))}
                  </div>
                </div>
              )}

              {}
              {selectedSeverity && (
                <div className="side-effect-section">
                  <label className="side-effect-section-label">Yan Etkiler (Birden fazla seçebilirsin)</label>
                  <div className="side-effect-options-list">
                    {sideEffectOptions.map((sideEffect) => (
                      <button
                        key={sideEffect}
                        className={`side-effect-option-item ${selectedSideEffects.includes(sideEffect) ? 'selected' : ''}`}
                        onClick={() => toggleSideEffect(sideEffect)}
                      >
                        <span>{sideEffect}</span>
                        {selectedSideEffects.includes(sideEffect) && (
                          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className="side-effect-check-icon">
                            <polyline points="20 6 9 17 4 12"></polyline>
                          </svg>
                        )}
                      </button>
                    ))}
                  </div>
                </div>
              )}
            </div>
            <div className="side-effect-modal-footer">
              <button 
                className="side-effect-save-button"
                onClick={handleSideEffectSave}
                disabled={isSavingSideEffect || !selectedMedicineForSideEffect || !selectedSeverity || selectedSideEffects.length === 0}
              >
                {isSavingSideEffect ? 'Kaydediliyor...' : 'Kaydet'}
              </button>
            </div>
          </div>
        </div>
      )}

      {}
      {showSideEffectListModal && (
        <div className="side-effect-list-modal-overlay" onClick={handleSideEffectListModalClose}>
          <div className="side-effect-list-modal" onClick={(e) => e.stopPropagation()}>
            <div className="side-effect-list-modal-header">
              <h3 className="side-effect-list-modal-title">Yan Etkiler</h3>
              <button 
                className="side-effect-list-modal-close"
                onClick={handleSideEffectListModalClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"></line>
                  <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
              </button>
            </div>
            <div className="side-effect-list-modal-content">
              {savedSideEffects.length === 0 ? (
                <div className="side-effect-list-empty">
                  <svg width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" style={{ opacity: 0.3, marginBottom: '16px' }}>
                    <path d="M9 12l2 2 4-4"></path>
                    <path d="M21 12c-1 0-3-1-3-3s2-3 3-3 3 1 3 3-2 3-3 3"></path>
                    <path d="M3 12c1 0 3-1 3-3s-2-3-3-3-3 1-3 3 2 3 3 3"></path>
                    <path d="M12 21c0-1-1-3-3-3s-3 2-3 3 1 3 3 3 3-2 3-3z"></path>
                    <path d="M12 3c0 1-1 3-3 3S6 4 6 3s1-3 3-3 3 2 3 3z"></path>
                  </svg>
                  <p>Henüz yan etki kaydı bulunmamaktadır.</p>
                  <p style={{ fontSize: '14px', color: '#66747F', marginTop: '8px' }}>Ayarlar'dan yan etki ekleyebilirsiniz.</p>
                </div>
              ) : (
                <div className="side-effect-list-items">
                  {savedSideEffects.map((item) => {
                    const severityOption = severityOptions.find(opt => opt.value === item.severity);
                    const severityColors = {
                      mild: '#10B981',
                      moderate: '#F59E0B',
                      severe: '#EF4444',
                      critical: '#DC2626'
                    };
                    const severityBgColors = {
                      mild: '#D1FAE5',
                      moderate: '#FEF3C7',
                      severe: '#FEE2E2',
                      critical: '#FEE2E2'
                    };
                    return (
                      <div key={item.id} className="side-effect-list-item">
                        <div className="side-effect-list-item-header">
                          <div className="side-effect-list-item-medicine">
                            <img 
                              src={getMedicineIcon(item.medicine?.type || 'tablet')} 
                              alt={item.medicine?.name || 'İlaç'} 
                              className="side-effect-list-medicine-icon" 
                            />
                            <div className="side-effect-list-medicine-info">
                              <div className="side-effect-list-medicine-name">
                                {item.medicine?.name || 'Bilinmeyen İlaç'}
                              </div>
                              <div className="side-effect-list-item-date">
                                {new Date(item.date).toLocaleDateString('tr-TR', {
                                  day: '2-digit',
                                  month: 'long',
                                  year: 'numeric'
                                })}
                              </div>
                            </div>
                          </div>
                          <div 
                            className="side-effect-list-severity-badge"
                            style={{
                              backgroundColor: severityBgColors[item.severity] || severityBgColors.mild,
                              color: severityColors[item.severity] || severityColors.mild
                            }}
                          >
                            {severityOption?.label || item.severity}
                          </div>
                        </div>
                        <div className="side-effect-list-item-effects">
                          <div className="side-effect-list-effects-label">Yaşanan Yan Etkiler:</div>
                          <div className="side-effect-list-effects-tags">
                            {item.sideEffects.map((effect, index) => (
                              <span key={index} className="side-effect-list-effect-tag">
                                {effect}
                              </span>
                            ))}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {}
      {showExtraHealthModal && (
        <div className="extra-health-modal-overlay" onClick={handleExtraHealthClose}>
          <div className="extra-health-modal" onClick={(e) => e.stopPropagation()}>
            <div className="extra-health-modal-header">
              <h3 className="extra-health-modal-title">Ekstra Sağlık Bilgilerini Ekle</h3>
              <button 
                className="extra-health-modal-close"
                onClick={handleExtraHealthClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"></line>
                  <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
              </button>
            </div>
            <div className="extra-health-modal-content">
              {}
              <div className="extra-health-field">
                <label className="extra-health-label">TC Kimlik No</label>
                <input
                  type="text"
                  className="extra-health-input"
                  value={healthData.tcKimlikNo}
                  onChange={(e) => {
                    
                    const value = e.target.value.replace(/\D/g, '').slice(0, 11);
                    setHealthData(prev => ({ ...prev, tcKimlikNo: value }));
                  }}
                  placeholder="TC Kimlik Numaranızı giriniz"
                  maxLength="11"
                />
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Kan Grubu</label>
                <select
                  className="extra-health-select"
                  value={healthData.bloodType}
                  onChange={(e) => setHealthData(prev => ({ ...prev, bloodType: e.target.value }))}
                >
                  <option value="">Seçiniz</option>
                  {bloodTypes.map(type => (
                    <option key={type} value={type}>{type}</option>
                  ))}
                </select>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Sigara İçiyor musunuz?</label>
                <div className="extra-health-toggle-group">
                  <button
                    type="button"
                    className={`extra-health-toggle ${healthData.smokes === true ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, smokes: true }))}
                  >
                    Evet
                  </button>
                  <button
                    type="button"
                    className={`extra-health-toggle ${healthData.smokes === false ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, smokes: false, cigarettesPerDay: '' }))}
                  >
                    Hayır
                  </button>
                </div>
                {healthData.smokes && (
                  <div className="extra-health-nested-field">
                    <input
                      type="number"
                      className="extra-health-input"
                      value={healthData.cigarettesPerDay}
                      onChange={(e) => setHealthData(prev => ({ ...prev, cigarettesPerDay: e.target.value }))}
                      placeholder="Miktar"
                      min="1"
                    />
                    <select
                      className="extra-health-select-small"
                      value={healthData.cigarettesUnit}
                      onChange={(e) => setHealthData(prev => ({ ...prev, cigarettesUnit: e.target.value }))}
                    >
                      <option value="adet">Adet</option>
                      <option value="paket">Paket</option>
                    </select>
                  </div>
                )}
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Alkol İçiyor musunuz?</label>
                <div className="extra-health-toggle-group">
                  <button
                    type="button"
                    className={`extra-health-toggle ${healthData.drinksAlcohol === true ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, drinksAlcohol: true }))}
                  >
                    Evet
                  </button>
                  <button
                    type="button"
                    className={`extra-health-toggle ${healthData.drinksAlcohol === false ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, drinksAlcohol: false }))}
                  >
                    Hayır
                  </button>
                </div>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Daha Önce Koronavirüs Geçirdiniz mi?</label>
                <div className="extra-health-toggle-group">
                  <button
                    type="button"
                    className={`extra-health-toggle ${healthData.hadCovid === true ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, hadCovid: true }))}
                  >
                    Evet
                  </button>
                  <button
                    type="button"
                    className={`extra-health-toggle ${healthData.hadCovid === false ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, hadCovid: false }))}
                  >
                    Hayır
                  </button>
                </div>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Doğduğunuz İl</label>
                <select
                  className="extra-health-select"
                  value={healthData.birthCity}
                  onChange={(e) => setHealthData(prev => ({ ...prev, birthCity: e.target.value }))}
                >
                  <option value="">Seçiniz</option>
                  {turkishCities.map(city => (
                    <option key={city} value={city}>{city}</option>
                  ))}
                </select>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Solak mı Sağlak mısınız?</label>
                <div className="extra-health-toggle-group">
                  <button
                    className={`extra-health-toggle ${healthData.handedness === 'solak' ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, handedness: 'solak' }))}
                  >
                    Solak
                  </button>
                  <button
                    className={`extra-health-toggle ${healthData.handedness === 'sağlak' ? 'active' : ''}`}
                    onClick={() => setHealthData(prev => ({ ...prev, handedness: 'sağlak' }))}
                  >
                    Sağlak
                  </button>
                </div>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Geçirilmiş Ameliyatlar</label>
                <div className="extra-health-list-container">
                  {healthData.surgeries.map((surgery, index) => (
                    <div key={index} className="extra-health-list-item">
                      <span>{surgery}</span>
                      <button
                        className="extra-health-remove-btn"
                        onClick={() => removeSurgery(index)}
                      >
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <line x1="18" y1="6" x2="6" y2="18"></line>
                          <line x1="6" y1="6" x2="18" y2="18"></line>
                        </svg>
                      </button>
                    </div>
                  ))}
                  <div className="extra-health-add-container">
                    <input
                      type="text"
                      className="extra-health-input"
                      value={newSurgery}
                      onChange={(e) => setNewSurgery(e.target.value)}
                      onKeyPress={(e) => e.key === 'Enter' && addSurgery()}
                      placeholder="Ameliyat adı"
                    />
                    <button className="extra-health-add-btn" onClick={addSurgery}>
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <line x1="12" y1="5" x2="12" y2="19"></line>
                        <line x1="5" y1="12" x2="19" y2="12"></line>
                      </svg>
                    </button>
                  </div>
                </div>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Kronik Rahatsızlıklar</label>
                <div className="extra-health-list-container">
                  {healthData.chronicDiseases.map((disease, index) => (
                    <div key={index} className="extra-health-list-item">
                      <span>{disease}</span>
                      <button
                        className="extra-health-remove-btn"
                        onClick={() => removeChronicDisease(index)}
                      >
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <line x1="18" y1="6" x2="6" y2="18"></line>
                          <line x1="6" y1="6" x2="18" y2="18"></line>
                        </svg>
                      </button>
                    </div>
                  ))}
                  <div className="extra-health-add-container">
                    <input
                      type="text"
                      className="extra-health-input"
                      value={newChronicDisease}
                      onChange={(e) => setNewChronicDisease(e.target.value)}
                      onKeyPress={(e) => e.key === 'Enter' && addChronicDisease()}
                      placeholder="Rahatsızlık adı"
                    />
                    <button className="extra-health-add-btn" onClick={addChronicDisease}>
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <line x1="12" y1="5" x2="12" y2="19"></line>
                        <line x1="5" y1="12" x2="19" y2="12"></line>
                      </svg>
                    </button>
                  </div>
                </div>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Acil Durum Kişileri</label>
                <div className="extra-health-list-container">
                  {healthData.emergencyContacts.map((contact, index) => (
                    <div key={index} className="extra-health-contact-item">
                      <div className="extra-health-contact-info">
                        <span className="extra-health-contact-name">{contact.name}</span>
                        <span className="extra-health-contact-phone">{contact.phone}</span>
                        <span className="extra-health-contact-relationship">{contact.relationship}</span>
                      </div>
                      <button
                        className="extra-health-remove-btn"
                        onClick={() => removeEmergencyContact(index)}
                      >
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                          <line x1="18" y1="6" x2="6" y2="18"></line>
                          <line x1="6" y1="6" x2="18" y2="18"></line>
                        </svg>
                      </button>
                    </div>
                  ))}
                  <div className="extra-health-contact-form">
                    <input
                      type="text"
                      className="extra-health-input"
                      value={newEmergencyContact.name}
                      onChange={(e) => setNewEmergencyContact(prev => ({ ...prev, name: e.target.value }))}
                      placeholder="İsim"
                    />
                    <input
                      type="tel"
                      className="extra-health-input"
                      value={newEmergencyContact.phone}
                      onChange={(e) => setNewEmergencyContact(prev => ({ ...prev, phone: e.target.value }))}
                      placeholder="Telefon"
                    />
                    <input
                      type="text"
                      className="extra-health-input"
                      value={newEmergencyContact.relationship}
                      onChange={(e) => setNewEmergencyContact(prev => ({ ...prev, relationship: e.target.value }))}
                      placeholder="Yakınlık derecesi"
                    />
                    <button className="extra-health-add-btn" onClick={addEmergencyContact}>
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                        <line x1="12" y1="5" x2="12" y2="19"></line>
                        <line x1="5" y1="12" x2="19" y2="12"></line>
                      </svg>
                    </button>
                  </div>
                </div>
              </div>

              {}
              <div className="extra-health-field">
                <label className="extra-health-label">Acil Not</label>
                <textarea
                  className="extra-health-textarea"
                  value={healthData.acilNot}
                  onChange={(e) => setHealthData(prev => ({ ...prev, acilNot: e.target.value }))}
                  placeholder="Acil durumlarda bilinmesi gereken özel notlarınızı buraya yazabilirsiniz..."
                  rows="4"
                />
              </div>
            </div>
            <div className="extra-health-modal-footer">
              <button 
                className="extra-health-save-button"
                onClick={handleExtraHealthSave}
                disabled={isSavingHealthData}
              >
                {isSavingHealthData ? 'Kaydediliyor...' : 'Kaydet'}
              </button>
            </div>
          </div>
        </div>
      )}

      {}
      <AddMedicineModal
        isOpen={showModal && !showScheduleModal && !isTransitioning && !isProcessing}
        onClose={handleModalClose}
        onNext={handleModalNext}
        initialData={currentMedicineData}
      />

      {}
      <AddMedicineScheduleModal
        isOpen={showScheduleModal && !showModal && !isTransitioning && !isProcessing}
        onClose={handleScheduleModalClose}
        onBack={handleScheduleModalBack}
        onAddMedicine={handleAddMedicine}
        medicineData={currentMedicineData}
      />

      </div>

      {}
      {!showAiChatPopup && (
        <div className="ai-chat-floating-button" onClick={handleAiChatPopupOpen}>
          <img src={logoAI} alt="AI Chat" className="ai-chat-floating-icon" />
        </div>
      )}

      {}
      {showLocationConfirm && pendingLocation && (
        <div className="location-confirm-overlay" onClick={rejectLocation}>
          <div className="location-confirm-modal" onClick={(e) => e.stopPropagation()}>
            <div className="location-confirm-header">
              <h3 className="location-confirm-title">Konumunuzu Doğrulayın</h3>
              <button 
                className="location-confirm-close"
                onClick={rejectLocation}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"/>
                  <line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
              </button>
            </div>
            
            <div className="location-confirm-content">
              <div className="location-confirm-map-preview">
                <MapContainer
                  center={[pendingLocation.latitude, pendingLocation.longitude]}
                  zoom={15}
                  style={{ height: '250px', width: '100%', borderRadius: '12px', zIndex: 1 }}
                  scrollWheelZoom={false}
                >
                  <TileLayer
                    attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                    url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  />
                  <Marker 
                    position={[pendingLocation.latitude, pendingLocation.longitude]}
                    icon={createUserIcon()}
                  >
                    <Popup>
                      <div style={{ fontWeight: 'bold', color: '#27ae60' }}>Konumunuz</div>
                    </Popup>
                  </Marker>
                </MapContainer>
              </div>
              
              {userAddress && (
                <div className="location-confirm-address">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path>
                    <circle cx="12" cy="10" r="3"></circle>
                  </svg>
                  <span>{userAddress}</span>
                </div>
              )}
              
              <div className="location-confirm-coordinates">
                <span>Koordinatlar: {pendingLocation.latitude.toFixed(6)}, {pendingLocation.longitude.toFixed(6)}</span>
              </div>
              
              <div className="location-confirm-message">
                <p>Bu konum doğru mu? Yakınındaki eczaneleri bulmak için onaylayın.</p>
              </div>
            </div>
            
            <div className="location-confirm-actions">
              <button 
                className="location-confirm-button location-confirm-button-cancel"
                onClick={rejectLocation}
              >
                İptal
              </button>
              <button 
                className="location-confirm-button location-confirm-button-confirm"
                onClick={confirmLocation}
              >
                Evet, Bu Doğru
              </button>
            </div>
          </div>
        </div>
      )}

      {}
      {showAiChatPopup && (
        <div className="ai-chat-popup-overlay" onClick={handleAiChatPopupClose}>
          <div className="ai-chat-popup" onClick={(e) => e.stopPropagation()}>
            <div className="ai-chat-popup-header">
              <div className="ai-chat-popup-title">
                <img src={logoAI} alt="AI Logo" className="ai-chat-popup-logo" />
                <span>AI Asistan</span>
              </div>
              <button 
                className="ai-chat-popup-close"
                onClick={handleAiChatPopupClose}
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"/>
                  <line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
              </button>
            </div>
            
            <div className="ai-chat-popup-content">
              {}
              {!hasUserMessage && (
                <div className={`ai-chat-popup-welcome ${isWelcomeFading ? 'fade-out' : ''}`}>
                  <div className="ai-chat-popup-welcome-logo">
                    <img src={logoAI} alt="AI Logo" className="ai-logo" />
                  </div>
                  <div className="ai-chat-popup-welcome-text">
                    <p>ilaç kullanımı hakkında merak ettiklerini veya dikkat</p>
                    <p>etmen gerekenleri öğrenmek için bana</p>
                    <p>danışabilirsin.</p>
                  </div>
                </div>
              )}

              {}
              <div className="chat-messages" ref={chatMessagesRef}>
                {chatMessages.map((message, index) => {
                  
                  const aiMessageIndices = chatMessages.map((msg, idx) => msg.type === 'ai' ? idx : -1).filter(idx => idx !== -1);
                  const lastAiMessageIndex = aiMessageIndices[aiMessageIndices.length - 1];
                  const isLastAiMessage = message.type === 'ai' && index === lastAiMessageIndex;
                  
                  return (
                    <div key={message.id} className={`chat-message ${message.type}`}>
                      {message.type === 'ai' && (
                        <div className={`ai-avatar ${isLastAiMessage ? 'has-shadow' : ''}`}>
                          <img src={logoAI} alt="AI" className="ai-avatar-img" />
                        </div>
                      )}
                      <div className="message-content">
                        {message.medicineImage && (
                          <div className="message-medicine-image">
                            <img src={message.medicineImage} alt={message.text} style={{ maxWidth: '200px', borderRadius: '8px', marginBottom: '8px' }} />
                          </div>
                        )}
                        <div className="message-text">
                          {message.isHTML ? (
                            <div dangerouslySetInnerHTML={{ __html: message.text }} />
                          ) : (
                            <span className={message.isTyping ? 'typing-text' : ''}>
                              {(message.text || '').split('**').map((part, i) => 
                                i % 2 === 1 ? <strong key={i}>{part}</strong> : <span key={i}>{part}</span>
                              )}
                              {message.isTyping && <span className="typing-cursor">|</span>}
                            </span>
                          )}
                        </div>
                        <div className="message-time">
                          {message.timestamp.toLocaleTimeString('tr-TR', { 
                            hour: '2-digit', 
                            minute: '2-digit' 
                          })}
                        </div>
                      </div>
                    </div>
                  );
                })}
                
                {isTyping && (
                  <div className="chat-message ai">
                    <div className="ai-avatar has-shadow">
                      <img src={logoAI} alt="AI" className="ai-avatar-img" />
                    </div>
                    <div className="message-content">
                      <div className="typing-indicator">
                        <span></span>
                        <span></span>
                        <span></span>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              {}
              <form className="chat-input-form" onSubmit={handleChatSubmit}>
                <div className="chat-input-container">
                  <input
                    ref={chatInputRef}
                    type="text"
                    value={chatInput}
                    onChange={handleChatInputChange}
                    onKeyDown={handleChatKeyDown}
                    placeholder="Mesajınızı yazın..."
                    className="chat-input"
                    disabled={isTyping}
                  />
                  <button 
                    type="submit" 
                    className="chat-send-button"
                    disabled={!chatInput.trim() || isTyping}
                  >
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                      <path d="M22 2L11 13"/>
                      <path d="M22 2L15 22L11 13L2 9L22 2Z"/>
                    </svg>
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {}
      {showAlreadyConsultedPopup && (
        <div className="already-consulted-popup">
          <div className="already-consulted-popup-content">
            <div className="already-consulted-icon">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <path d="M12 2L2 7l10 5 10-5-10-5z"></path>
                <path d="M2 17l10 5 10-5"></path>
                <path d="M2 12l10 5 10-5"></path>
              </svg>
            </div>
            <p className="already-consulted-text">Zaten yapay zekaya danışıldı</p>
          </div>
        </div>
      )}

      {}
      {showHealthCardModal && (
        <div className="health-card-modal-overlay" onClick={handleHealthCardModalClose}>
          <div className="health-card-modal" onClick={(e) => e.stopPropagation()}>
            <div className="health-card-modal-header">
              <h3 className="health-card-modal-title">Sağlık Kartım</h3>
              <div className="health-card-modal-header-actions">
                <button 
                  className="health-card-modal-share"
                  onClick={handleExportHealthCardPDF}
                  title="PDF olarak indir/paylaş"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M4 12v8a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-8"></path>
                    <polyline points="16 6 12 2 8 6"></polyline>
                    <line x1="12" y1="2" x2="12" y2="15"></line>
                  </svg>
                  <span>Gönder</span>
                </button>
                <button 
                  className="health-card-modal-close"
                  onClick={handleHealthCardModalClose}
                >
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <line x1="18" y1="6" x2="6" y2="18"></line>
                    <line x1="6" y1="6" x2="18" y2="18"></line>
                  </svg>
                </button>
              </div>
            </div>
            <div className="health-card-modal-content">
              {}
              <div className="health-card-id">
                <div className="health-card-id-header">
                  <div className="health-card-id-logo">
                    <img src={logo} alt="Logo" />
                  </div>
                  <div className="health-card-id-title">Sağlık Kartı</div>
                </div>
                
                <div className="health-card-id-body">
                  {}
                  <div className="health-card-id-row">
                    <div className="health-card-id-label">Ad Soyad</div>
                    <div className="health-card-id-value">{userInfo?.name || 'Belirtilmemiş'}</div>
                  </div>

                  {}
                  {healthData.tcKimlikNo && (
                    <div className="health-card-id-row">
                      <div className="health-card-id-label">TC Kimlik No</div>
                      <div className="health-card-id-value">{healthData.tcKimlikNo}</div>
                    </div>
                  )}

                  {}
                  <div className="health-card-id-row">
                    <div className="health-card-id-label">Kilo / Boy</div>
                    <div className="health-card-id-value">
                      {(userInfo?.weightKg ?? userBodyData?.weight) || '--'} kg / {(userInfo?.heightCm ?? userBodyData?.height) || '--'} cm
                    </div>
                  </div>

                  {}
                  {healthData.bloodType && (
                    <div className="health-card-id-row">
                      <div className="health-card-id-label">Kan Grubu</div>
                      <div className="health-card-id-value">{healthData.bloodType}</div>
                    </div>
                  )}

                  {}
                  {addedMedicines.length > 0 && (
                    <div className="health-card-id-row">
                      <div className="health-card-id-label">Önemli İlaçlar</div>
                      <div className="health-card-id-value">
                        <div className="health-card-medicines-list">
                          {addedMedicines.slice(0, 3).map((medicine, index) => (
                            <span key={medicine.id || index} className="health-card-medicine-tag">
                              {medicine.name}
                            </span>
                          ))}
                          {addedMedicines.length > 3 && (
                            <span className="health-card-medicine-tag">+{addedMedicines.length - 3}</span>
                          )}
                        </div>
                      </div>
                    </div>
                  )}

                  {}
                  {healthData.chronicDiseases.length > 0 && (
                    <div className="health-card-id-row">
                      <div className="health-card-id-label">Kronik Hastalıklar</div>
                      <div className="health-card-id-value">
                        <div className="health-card-diseases-list">
                          {healthData.chronicDiseases.map((disease, index) => (
                            <span key={index} className="health-card-disease-tag">{disease}</span>
                          ))}
                        </div>
                      </div>
                    </div>
                  )}

                  {}
                  {healthData.emergencyContacts.length > 0 && (
                    <div className="health-card-id-row">
                      <div className="health-card-id-label">Acil Durum İletişim</div>
                      <div className="health-card-id-value">
                        <div className="health-card-contacts-list">
                          {healthData.emergencyContacts.map((contact, index) => (
                            <div key={index} className="health-card-contact-item">
                              <span className="health-card-contact-name">{contact.name}</span>
                              <span className="health-card-contact-phone">{contact.phone}</span>
                              {contact.relationship && (
                                <span className="health-card-contact-relation">({contact.relationship})</span>
                              )}
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  )}

                  {}
                  {healthData.surgeries.length > 0 && (
                    <div className="health-card-id-row">
                      <div className="health-card-id-label">Geçirilmiş Ameliyatlar</div>
                      <div className="health-card-id-value">
                        <div className="health-card-surgeries-list">
                          {healthData.surgeries.map((surgery, index) => (
                            <span key={index} className="health-card-surgery-tag">{surgery}</span>
                          ))}
                        </div>
                      </div>
                    </div>
                  )}

                  {}
                  {healthData.acilNot && (
                    <div className="health-card-id-row">
                      <div className="health-card-id-label">Acil Not</div>
                      <div className="health-card-id-value health-card-note-text">{healthData.acilNot}</div>
                    </div>
                  )}
                </div>

                <div className="health-card-id-footer">
                  <div className="health-card-id-note">Bu kart acil durumlarda kullanılabilir</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {}
      {medicineTimePopup && (
        <div className="medicine-time-popup">
          <div className="medicine-time-popup-content">
            <div className="medicine-time-popup-header">
              <button 
                className="medicine-time-popup-mute"
                onClick={toggleAlarmMute}
                title={isAlarmMuted ? "Sesi Aç" : "Sesi Kıs"}
              >
                {isAlarmMuted ? (
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M11 5L6 9H2v6h4l5 4V5z"></path>
                    <line x1="23" y1="9" x2="17" y2="15"></line>
                    <line x1="17" y1="9" x2="23" y2="15"></line>
                  </svg>
                ) : (
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M11 5L6 9H2v6h4l5 4V5z"></path>
                    <path d="M19.07 4.93a10 10 0 0 1 0 14.14M15.54 8.46a5 5 0 0 1 0 7.07"></path>
                    <path d="M15.54 8.46L19.07 4.93"></path>
                    <path d="M15.54 15.54L19.07 19.07"></path>
                  </svg>
                )}
              </button>
              <button 
                className="medicine-time-popup-close"
                onClick={handleMedicineTimePopupClose}
              >
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                  <line x1="18" y1="6" x2="6" y2="18"></line>
                  <line x1="6" y1="6" x2="18" y2="18"></line>
                </svg>
              </button>
            </div>
            <div className="medicine-time-popup-icon">
              <img 
                src={getMedicineIcon(medicineTimePopup.medicine.type)} 
                alt={medicineTimePopup.medicine.name}
                className="medicine-time-popup-icon-img"
              />
            </div>
            <h3 className="medicine-time-popup-title">İlaç Saati Geldi!</h3>
            <p className="medicine-time-popup-medicine-name">{medicineTimePopup.medicine.name}</p>
            <p className="medicine-time-popup-time">Saat: {medicineTimePopup.time}</p>
            <p className="medicine-time-popup-dosage">Doz: {medicineTimePopup.medicine.dosage}</p>
            <button 
              className="medicine-time-popup-button"
              onClick={handleMedicineTaken}
            >
              İlaç Alındı
            </button>
          </div>
        </div>
      )}

    </>
  );
};

export default DashboardPage;
