import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProxyManagerComponent } from './proxy-manager.component';

describe('ProxyManagerComponent', () => {
  let component: ProxyManagerComponent;
  let fixture: ComponentFixture<ProxyManagerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ProxyManagerComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ProxyManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
