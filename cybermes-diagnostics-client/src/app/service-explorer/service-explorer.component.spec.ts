import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ServiceExplorerComponent } from './service-explorer.component';

describe('ServiceExplorerComponent', () => {
  let component: ServiceExplorerComponent;
  let fixture: ComponentFixture<ServiceExplorerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ServiceExplorerComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ServiceExplorerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
