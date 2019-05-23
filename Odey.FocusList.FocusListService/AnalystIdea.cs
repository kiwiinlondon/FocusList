using Odey.FocusList.Contracts;
using E=Odey.Framework.Keeley.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Odey.FocusList.FocusListService
{
    public class AnalystIdeaManager
    {

        public AnalystIdeaManager(E.KeeleyModel context)
        {
            _context = context;
        }
        //    public void Add(List<int issuerId, bool isLong, DateTime effectiveFromDate, DateTime effectiveToDate)
        //    {

        //    }

        private E.KeeleyModel _context;

        public void Add(AnalystIdea dto)
        {


            if (dto.FocusLists == null)
            {
                dto.FocusLists = _context.FocusLists.Where(a => a.IssuerId == dto.IssuerId).Select(a => new FocusListDTO()
                {
                    AnalystId = a.AnalystId,
                    EffectiveFromDate = a.InDate,
                    EffectiveToDate = a.OutDate ?? new DateTime(9999, 12, 31),
                    InPrice = a.InPrice,
                    InstrumentMarketId = a.InstrumentMarketId,
                    IsLong = a.IsLong
                }).ToList();
            }
            var newIdeas = ResolveNew(dto);
            var existingIdeas = _context.AnalystIdea2.Where(a => a.IssuerId == dto.IssuerId).ToList();
            Match(existingIdeas, newIdeas);
         
        }

        private void Match(List<E.AnalystIdea2> existingIdeas, List<E.AnalystIdea2> newIdeas)
        {
            Dictionary<E.AnalystIdea2, E.AnalystIdea2> matchedIdeas = new Dictionary<E.AnalystIdea2, E.AnalystIdea2>();
            List<E.AnalystIdea2> ideasToAdd = new List<E.AnalystIdea2>();
            foreach (var newIdea in newIdeas)
            {
                var exists = existingIdeas.FirstOrDefault(a => a.IsLong = newIdea.IsLong && a.EffectiveFromDate == newIdea.EffectiveFromDate && a.FocusListId == newIdea.FocusListId && a.AnalystId == newIdea.AnalystId && a.OriginatiorId == newIdea.OriginatiorId);
                if (exists != null)
                {
                    matchedIdeas.Add(exists,newIdea);
                }
                else
                {
                    ideasToAdd.Add(newIdea);
                }
            }
            List<E.AnalystIdea2> ideasToRemove = new List<E.AnalystIdea2>();
            foreach(var existingIdea in existingIdeas)
            {
                if (!matchedIdeas.ContainsKey(existingIdea))
                {
                    ideasToRemove.Add(existingIdea);
                }
            }

            foreach(var matched in matchedIdeas)
            {
                var existing = matched.Key;
                var toApply = matched.Value;
                existing.Analyst = toApply.Analyst;
                existing.EffectiveToDate = toApply.EffectiveToDate;
                existing.FocusList = toApply.FocusList;
                existing.Originator = toApply.Originator;                
            }
            foreach(var ideaToAdd in ideasToAdd)
            {
                _context.AnalystIdea2.Add(ideaToAdd);
            }
            foreach (var ideaToRemove in ideasToRemove)
            {
                _context.AnalystIdea2.Remove(ideaToRemove);
            }
        }

        private List<E.AnalystIdea2> ResolveNew(AnalystIdea dto)
        {

            var dtos = dto.FocusLists.Concat<IAnalystIdea>(dto.Analysts).Concat<IAnalystIdea>(dto.Originators);
            var longs = ResolveForLongShort(dto, dtos, true);
            var shorts = ResolveForLongShort(dto, dtos, false);
            return longs.Concat(shorts).ToList();

        }

        private static readonly DateTime EOT = new DateTime(9999, 12, 31);

        private List<E.AnalystIdea2> ResolveForLongShort(AnalystIdea dto,IEnumerable<IAnalystIdea> dtos, bool isLong)
        {
            dtos = dtos.Where(a => a.IsLong == isLong).ToList();
            List<DateTime> datePoints = new List<DateTime>();

            foreach (var newDTO in dtos.OrderBy(a => a.EffectiveFromDate).ThenBy(a => a.EffectiveToDate))
            {
                AddDatePoint(datePoints, newDTO.EffectiveFromDate);
                DateTime effectiveToDate = newDTO.EffectiveToDate;
                if (effectiveToDate < EOT)
                {
                    effectiveToDate = effectiveToDate.AddDays(1);
                }
                AddDatePoint(datePoints, effectiveToDate);
            }

            List<E.AnalystIdea2> analystIdeas = new List<E.AnalystIdea2>();
            DateTime? previous = null;
            foreach (DateTime datePoint in datePoints)
            {
                if (previous != null)
                {
                    DateTime effectiveToDate = datePoint;
                    if (effectiveToDate != EOT)
                    {
                        effectiveToDate = datePoint.AddDays(-1);
                    }
                    analystIdeas.Add(new E.AnalystIdea2() { EffectiveFromDate = previous.Value, EffectiveToDate = effectiveToDate, IsLong = isLong });
                }
                previous = datePoint;
            }

            foreach (var analystIdea in analystIdeas)
            {
                analystIdea.Originator = GetOriginator(analystIdea, dto);
                analystIdea.FocusList = GetFocusList(analystIdea, dto);
                analystIdea.Analyst = GetAnalyst(analystIdea, dto);
            }
            return analystIdeas;
        }

        private E.Originator GetOriginator(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            OriginatorDTO originatorDTO = (OriginatorDTO)GetOverlapping(analystIdea, dto.Originators, analystIdea.IsLong);

            if (originatorDTO != null)
            {
                var originator = _context.Originators.FirstOrDefault(a => a.InternalOriginatorId == originatorDTO.InternalOriginatorId && a.ExternalOriginatorId == originatorDTO.ExternalOriginatorId && a.InternalOriginatorId2 == originatorDTO.InternalOriginatorId2  );
                if (originator == null)
                {
                    originator = new E.Originator() { ExternalOriginatorId = originatorDTO.ExternalOriginatorId, InternalOriginatorId = originatorDTO.InternalOriginatorId, InternalOriginatorId2 = originatorDTO.InternalOriginatorId2 };
                    _context.Originators.Add(originator);
                }
                return originator;
            }
            return null;
        }

        private E.ApplicationUser GetAnalyst(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            AnalystDTO analystDTO = (AnalystDTO)GetOverlapping(analystIdea, dto.Analysts, analystIdea.IsLong);

            if (analystDTO != null)
            {                
                var analyst = _context.ApplicationUsers.FirstOrDefault(a => a.UserID == analystDTO.AnalystId);
                if (analyst == null)
                {
                    throw new ApplicationException("Cant find Application User");
                }
                return analyst;
            }
            return null;
        }

        private E.FocusList GetFocusList(E.AnalystIdea2 analystIdea, AnalystIdea dto)
        {
            FocusListDTO focusListDTO = (FocusListDTO)GetOverlapping(analystIdea, dto.FocusLists, analystIdea.IsLong);

            if (focusListDTO != null)
            {
                var focusList = _context.FocusLists.FirstOrDefault(a => a.InstrumentMarketId ==  focusListDTO.InstrumentMarketId && a.InDate == focusListDTO.EffectiveFromDate && a.AnalystId == focusListDTO.AnalystId );
                if (focusList == null)
                {
                    throw new ApplicationException("Cant find focus list");
                }
                return focusList;
            }
            return null;
        }

        private IAnalystIdea GetOverlapping(E.AnalystIdea2 analystIdea, IEnumerable<IAnalystIdea> inputs,bool isLong)
        {
            var overlapping = inputs.Where(a => a.EffectiveFromDate <= analystIdea.EffectiveFromDate && a.EffectiveToDate <= analystIdea.EffectiveToDate && a.IsLong == isLong).ToList();
            if (overlapping.Count==0)
            {
                return null;
            }
            if (overlapping.Count>1)
            {
                throw new ApplicationException("Too many inputs received for overlapping");
            }
            return overlapping.First();
        }

        private void AddDatePoint(List<DateTime> datePoints, DateTime dateToAdd)
        {
            if (!datePoints.Contains(dateToAdd))
            {
                datePoints.Add(dateToAdd);
            }
        }

    }
}