import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private apiUrl = environment.apiUrl + '/invoice';

  constructor(private http: HttpClient) { }

  uploadInvoice(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('File', file);
    return this.http.post(`${this.apiUrl}/upload`, formData);
  }

  getDashboard(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }
}