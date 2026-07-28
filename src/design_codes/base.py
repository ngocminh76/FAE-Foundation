"""
Abstract Base Class for Multi-Standard Code Verification (TCVN, ACI 318, Eurocode)
"""

from abc import ABC, abstractmethod
from typing import Dict, Any
from src.core.models import TowerFoundationProject

class BaseCodeChecker(ABC):
    def __init__(self, project: TowerFoundationProject, fea_results: Dict[str, Any]):
        self.project = project
        self.fea_results = fea_results
        
    @property
    @abstractmethod
    def code_name(self) -> str:
        """Tên tiêu chuẩn thiết kế (ví dụ: TCVN 5574:2018 / TCVN 9362:2012)"""
        pass

    @abstractmethod
    def check_soil_bearing(self) -> Dict[str, Any]:
        """Kiểm tra áp lực đất nền Pmax <= Rtc (Sức chịu tải đất)"""
        pass

    @abstractmethod
    def check_uplift_stability(self) -> Dict[str, Any]:
        """Kiểm tra chống nhổ móng (Uplift resistance & Soil cone stability)"""
        pass

    @abstractmethod
    def design_beam_flexure(self) -> Dict[str, Any]:
        """Tính toán cốt thép chịu uốn As cho 4 dầm sườn móng"""
        pass

    @abstractmethod
    def design_slab_flexure(self) -> Dict[str, Any]:
        """Tính toán cốt thép chịu uốn As cho bản móng bè"""
        pass

    @abstractmethod
    def run_all_checks(self) -> Dict[str, Any]:
        """Chạy tất cả các bước kiểm tra tiêu chuẩn và tổng hợp kết quả chi tiết"""
        pass
