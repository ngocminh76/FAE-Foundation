"""
Abstract Base Class for Multi-Standard Code Verification
Expanded with Stub Column, Anchor Bolts, and Punching Shear Calculations
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
        pass

    @abstractmethod
    def check_soil_bearing(self) -> Dict[str, Any]:
        """Kiểm tra áp lực đất nền Pmax <= Rtc và đường ranh giới hẫng móng Pmin"""
        pass

    @abstractmethod
    def check_uplift_stability(self) -> Dict[str, Any]:
        """Kiểm tra ổn định chống nhổ móng K_nhổ >= 1.3"""
        pass

    @abstractmethod
    def check_stub_columns(self) -> Dict[str, Any]:
        """Tính toán và kiểm tra 4 Cổ Cột: Thép dọc As_col, Thép đai Asw_col (Nén uốn xiên / Kéo uốn xiên)"""
        pass

    @abstractmethod
    def check_anchor_bolts(self) -> Dict[str, Any]:
        """Kiểm tra khả năng chịu kéo nhổ và cắt của cụm 4 Bu-lông Neo trên đỉnh cổ cột"""
        pass

    @abstractmethod
    def check_punching_shear(self) -> Dict[str, Any]:
        """Kiểm tra đâm thủng (Punching shear) của 4 cổ cột lên bản móng bè"""
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
        pass
