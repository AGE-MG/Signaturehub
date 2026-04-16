import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";

@Injectable({
    providedIn: 'root'
})

export class DashboardService {
    private readonly API_URL = `${environment.apiUrl}/dashboard`
}
