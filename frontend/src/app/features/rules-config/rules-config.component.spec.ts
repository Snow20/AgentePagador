import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RulesConfigComponent } from './rules-config.component';

describe('RulesConfigComponent', () => {
  let component: RulesConfigComponent;
  let fixture: ComponentFixture<RulesConfigComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RulesConfigComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RulesConfigComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
