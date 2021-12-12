import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { User } from '../models/User';
import { BASE_URL } from '../app.constants';

@Injectable({ providedIn: 'root' })
export class AuthenticationService {
    private currentUserSubject: BehaviorSubject<User|null>;
    public currentUser: Observable<User|null>;

    constructor(private http: HttpClient) {
        let currentUserString = String(localStorage.getItem('currentUser'));
        this.currentUserSubject = new BehaviorSubject<User|null>(JSON.parse(currentUserString));
        this.currentUser = this.currentUserSubject.asObservable();
    }

    public get currentUserValue(): User|null {
        return this.currentUserSubject.value;
    }


    public get currentTokenValue(): string|null{
        return localStorage.getItem('jwtToken');
    }

    login(username: string, password: string) {

        // uso shareReplay per effettuare multicast, cioè da un Observable generare 2 eventi
        // in uno leggo lo header, nel secondo interpreto il body e lo salvo
        const source = this.http.post<any>(`${BASE_URL}/api/Token`, { username, password }, {observe: 'response'})
            .pipe(shareReplay());
    
        return source
            .pipe(
                map(response => 
                    {
                        // store user details and jwt token in local storage to keep user logged in between page refreshes
                        localStorage.setItem('currentUser', JSON.stringify(response.body));
                        localStorage.setItem('jwtToken', response.headers.get('x-token')!);
                        this.currentUserSubject.next(response.body);
                        
                        return response.body;
                    })
            )
    }

    logout() {
        // remove user from local storage to log user out
        localStorage.removeItem('currentUser');
        localStorage.removeItem('jwtToken');
        this.currentUserSubject.next(null);
    }
}