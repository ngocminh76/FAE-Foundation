"""
Multi-Standard Code Verification Manager
"""

from typing import Dict, Any, List
from src.core.models import TowerFoundationProject
from src.design_codes.tcvn import TCVNCodeChecker
from src.design_codes.aci318 import ACI318CodeChecker
from src.design_codes.eurocode import EurocodeChecker

class CodeCheckerManager:
    def __init__(self, project: TowerFoundationProject, fea_results: Dict[str, Any]):
        self.project = project
        self.fea_results = fea_results
        self.checkers = {
            "TCVN": TCVNCodeChecker(project, fea_results),
            "ACI318": ACI318CodeChecker(project, fea_results),
            "EUROCODE": EurocodeChecker(project, fea_results)
        }

    def check_standard(self, code_key: str = "TCVN") -> Dict[str, Any]:
        """Kiểm tra theo 1 tiêu chuẩn chỉ định (TCVN, ACI318, EUROCODE)"""
        checker = self.checkers.get(code_key.upper(), self.checkers["TCVN"])
        return checker.run_all_checks()

    def compare_all_standards(self) -> Dict[str, Any]:
        """So sánh kết quả kiểm tra đồng thời trên cả 3 tiêu chuẩn (TCVN, ACI 318, Eurocode)"""
        results = {}
        for key, checker in self.checkers.items():
            results[key] = checker.run_all_checks()
        return results
