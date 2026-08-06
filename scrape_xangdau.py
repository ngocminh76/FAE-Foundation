import time
import pandas as pd
from selenium.webdriver.common.by import By
from bs4 import BeautifulSoup
import undetected_chromedriver as uc
import os
import io
import re
from datetime import datetime
import sys

# Ép kiểu xuất ra terminal thành UTF-8
if sys.stdout.encoding != 'utf-8':
    sys.stdout.reconfigure(encoding='utf-8')

SEARCH_URL = "https://thuvienphapluat.vn/page/tim-van-ban.aspx?keyword=%C4%91i%E1%BB%81u%20h%C3%A0nh%20gi%C3%A1%20b%C3%A1n%20x%C4%83ng%20d%E1%BA%A7u&type=3&match=True&area=0"

def get_driver():
    options = uc.ChromeOptions()
    options.add_argument("--window-size=1280,720")
    driver = uc.Chrome(options=options, version_main=150)
    return driver

def extract_document_links(driver):
    soup = BeautifulSoup(driver.page_source, 'html.parser')
    links = []
    for a in soup.find_all('a', href=True):
        href = a['href']
        if '/cong-van/' in href or '/van-ban/' in href:
            if not href.startswith('http'):
                href = 'https://thuvienphapluat.vn' + href
            url_lower = href.lower()
            if 'xang-dau' in url_lower or 'gia-co-so' in url_lower:
                links.append(href)
    return list(set(links))

def main():
    driver = get_driver()
    try:
        driver.get(SEARCH_URL)
        input("\n[QUAN TRỌNG] Vui lòng tự xác minh Cloudflare trên trình duyệt. Khi trang tải xong, nhấn ENTER ở đây để tiếp tục...")
        
        all_links = []
        page_num = 1
        
        print("\nBƯỚC 1: ĐANG THU THẬP DANH SÁCH CÔNG VĂN...")
        while True:
            page_url = SEARCH_URL + f"&page={page_num}"
            driver.get(page_url)
            time.sleep(3)
            
            current_links = extract_document_links(driver)
            if len(current_links) == 0:
                print(f"-> Đã quét đến trang cuối cùng ({page_num - 1}).")
                break
                
            all_links.extend(current_links)
            print(f"-> Đã quét xong trang {page_num}, thu được {len(all_links)} link...")
            page_num += 1
                
        # Loại bỏ các link trùng lặp
        all_links = list(set(all_links))
        
        print(f"\nTổng số link cần trích xuất: {len(all_links)} công văn. Quá trình này sẽ mất khá nhiều thời gian.")
        
        master_gia_co_so = []
        master_gia_the_gioi = []
        
        for idx, link in enumerate(all_links):
            print(f"\n[{idx+1}/{len(all_links)}] Đang xử lý: {link}")
            driver.get(link)
            time.sleep(2) 
            
            try:
                soup = BeautifulSoup(driver.page_source, 'html.parser')
                content_div = soup.find('div', class_='content1')
                if not content_div:
                    content_div = soup.find('div', id='divContentDoc')
                
                # Sửa lỗi 1: Lấy ngày từ toàn bộ text của công văn để không bị sót
                extracted_date = 'N/A'
                if content_div:
                    # Tìm tất cả các cụm ngày tháng năm
                    dates_str = re.findall(r'(\d{1,2})[/\-](\d{1,2})[/\-](\d{4})', content_div.text)
                    if dates_str:
                        try:
                            # Lấy ngày lớn nhất (thường là ngày ban hành/áp dụng)
                            parsed_dates = [datetime(int(y), int(m), int(d)) for d, m, y in dates_str]
                            extracted_date = max(parsed_dates).strftime('%d/%m/%Y')
                        except:
                            extracted_date = f"{dates_str[-1][0]}/{dates_str[-1][1]}/{dates_str[-1][2]}"

                html_to_parse = str(content_div) if content_div else driver.page_source
                tables = pd.read_html(io.StringIO(html_to_parse))
                print(f"  -> Tìm thấy {len(tables)} bảng.")
                
                matched_any = False
                for t_idx, tbl in enumerate(tables):
                    tbl_str = tbl.to_string().lower()
                    tbl_str_clean = tbl_str.replace(" ", "").replace("\n", "").replace("\t", "")
                    
                    if 'giácơsở' in tbl_str_clean or 'giábánlẻ' in tbl_str_clean or 'xănge5' in tbl_str_clean:
                        # Chèn cột ngày vào vị trí đầu tiên
                        tbl.insert(0, 'Ngày', extracted_date)
                        tbl['Source_Link'] = link
                        master_gia_co_so.append(tbl)
                        print(f"    + Đã lấy Bảng Giá Cơ Sở")
                        matched_any = True
                        
                    elif 'thếgiới' in tbl_str_clean or 'x92' in tbl_str_clean or 'x95' in tbl_str_clean:
                        tbl.insert(0, 'Ngày', extracted_date)
                        tbl['Source_Link'] = link
                        master_gia_the_gioi.append(tbl)
                        print(f"    + Đã lấy Bảng Giá Thế Giới")
                        matched_any = True
                
                if not matched_any:
                    print(f"  -> Không khớp định dạng bảng xăng dầu.")
                        
            except ValueError:
                print(f"  -> Không tìm thấy bảng nào.")
            except Exception as e:
                print(f"  -> Lỗi khi phân tích: {e}")
                
        print("\nBƯỚC 3: LƯU DỮ LIỆU RA EXCEL...")
        output_file = "LichSuGiaXangDau.xlsx"
        while True:
            try:
                with pd.ExcelWriter(output_file, engine='openpyxl') as writer:
                    if master_gia_co_so:
                        df_gia_co_so = pd.concat(master_gia_co_so, ignore_index=True)
                        # Dọn dẹp các dòng tiêu đề lặp lại
                        if len(df_gia_co_so.columns) > 1:
                            df_gia_co_so = df_gia_co_so[~df_gia_co_so.iloc[:, 1].astype(str).str.contains(r'Mặt hàng|^\(\d+\)$|^nan$', regex=True, na=False, case=False)]
                        df_gia_co_so.to_excel(writer, sheet_name="GiaCoSo", index=False)
                    else:
                        pd.DataFrame().to_excel(writer, sheet_name="GiaCoSo")
                        
                    if master_gia_the_gioi:
                        df_gia_the_gioi = pd.concat(master_gia_the_gioi, ignore_index=True)
                        # Dọn dẹp các dòng tiêu đề lặp lại
                        if len(df_gia_the_gioi.columns) > 0:
                            df_gia_the_gioi = df_gia_the_gioi[~df_gia_the_gioi.iloc[:, 0].astype(str).str.contains(r'TT|^nan$', regex=True, na=False, case=False)]
                        if len(df_gia_the_gioi.columns) > 1:
                            df_gia_the_gioi = df_gia_the_gioi[~df_gia_the_gioi.iloc[:, 1].astype(str).str.contains(r'Ngày|^nan$', regex=True, na=False, case=False)]
                        df_gia_the_gioi.to_excel(writer, sheet_name="GiaTheGioi", index=False)
                    else:
                        pd.DataFrame().to_excel(writer, sheet_name="GiaTheGioi")
                print(f"[THÀNH CÔNG] Đã lưu file Excel: {output_file}")
                break
            except PermissionError:
                input(f"\n[LỖI] Đóng file {output_file} trong Excel rồi nhấn ENTER để thử lưu lại...")
            except Exception as e:
                print(f"Lỗi không xác định khi lưu file: {e}")
                break
                
    finally:
        driver.quit()

if __name__ == "__main__":
    main()
